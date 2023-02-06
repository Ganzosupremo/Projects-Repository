using ComputeShader;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using static SlimeSimulation.SlimeSettingsSO;
using Random = UnityEngine.Random;

namespace SlimeSimulation
{
    public class Simulation : MonoBehaviour
    {
        public enum SpawnMode 
        { 
            Random, 
            Point, 
            InwardCircle,
            RandomCircle,
            InwardFillSquare,
            RandomFillSquare
        }

        const int updateKernel = 0;
        const int diffuseMapKernel = 1;
        const int colourKernel = 2;

        public UnityEngine.ComputeShader compute;
        public UnityEngine.ComputeShader drawAgentsCS;

        public bool chooseRandomSettings;
        public SlimeSettingsSO settings;

        [Header("Display Settings")]
        public bool showAgentsOnly;
        public FilterMode filterMode = FilterMode.Point;
        public GraphicsFormat format = ComputeHelper.defaultGraphicsFormat;

        [SerializeField, HideInInspector] protected RenderTexture trailMap;
        [SerializeField, HideInInspector] protected RenderTexture diffusedTrailMap;
        [SerializeField, HideInInspector] protected RenderTexture displayTexture;

        ComputeBuffer agentBuffer;
        ComputeBuffer settingsBuffer;
        Texture2D colourMapTexture;

        protected virtual void Start()
        {
            Init();
            transform.GetComponentInChildren<MeshRenderer>().material.mainTexture = displayTexture;
        }

        private void Init()
        {
            if (chooseRandomSettings)
            {
                settings = GameResources.Instance.slimeSettingsSOArray[Random.Range(0, GameResources.Instance.slimeSettingsSOArray.Length)];
            }

            // Create render textures
            ComputeHelper.CreateRenderTexture(ref trailMap, settings.width, settings.height, filterMode, format);
            ComputeHelper.CreateRenderTexture(ref diffusedTrailMap, settings.width, settings.height, filterMode, format);
            ComputeHelper.CreateRenderTexture(ref displayTexture, settings.width, settings.height, filterMode, format);

            // Assign textures
            compute.SetTexture(updateKernel, "TrailMap", trailMap);
            compute.SetTexture(diffuseMapKernel, "TrailMap", trailMap);
            compute.SetTexture(diffuseMapKernel, "DiffusedTrailMap", diffusedTrailMap);
            compute.SetTexture(colourKernel, "ColourMap", displayTexture);
            compute.SetTexture(colourKernel, "TrailMap", trailMap);

            // Create agents with initial positions and angles
            Agent[] agents = new Agent[settings.numAgents];
            for (int i = 0; i < agents.Length; i++)
            {
                Vector2 centre = new(settings.width / 2, settings.height / 2);
                Vector2 startPos = Vector2.zero;
                float randomAngle = Random.value * Mathf.PI * 2;
                float angle = 0;

                switch (settings.spawnMode)
                {
                    case SpawnMode.Random:
                        startPos = new(Random.Range(0, settings.width), Random.Range(0, settings.height));
                        angle = randomAngle;
                        break;
                    case SpawnMode.Point:
                        startPos = centre;
                        angle = randomAngle;
                        break;
                    case SpawnMode.InwardCircle:
                        startPos = centre + 0.5f * settings.height * Random.insideUnitCircle;
                        angle = Mathf.Atan2((centre - startPos).normalized.y, (centre - startPos).normalized.x);
                        break;
                    case SpawnMode.RandomCircle:
                        startPos = centre + 0.15f * settings.height * Random.insideUnitCircle;
                        angle = randomAngle;
                        break;
                    case SpawnMode.InwardFillSquare:
                        startPos = centre + 0.69f * settings.width * Random.insideUnitCircle;
                        angle = Mathf.Atan((centre - startPos).normalized.x);
                        break;
                    case SpawnMode.RandomFillSquare:
                        startPos = centre * 1.69f * settings.width * Random.insideUnitCircle;
                        angle = randomAngle;
                        break;
                    default:
                        break;
                }

                Vector3Int speciesMask;
                int speciesIndex = 0;
                int numSpecies = settings.speciesSettings.Length;

                if (numSpecies == 1)
                {
                    speciesMask = Vector3Int.one;
                }
                else
                {
                    int species = Random.Range(1, numSpecies + 1);
                    speciesIndex = species - 1;
                    speciesMask = new((species == 1) ? 1 : 0, (species == 2) ? 1 : 0, (species == 3) ? 1 : 0);
                }

                agents[i] = new Agent() 
                { 
                    position = startPos, 
                    angle = angle, 
                    speciesMask = speciesMask, 
                    speciesIndex = speciesIndex 
                };
            }

            ComputeHelper.CreateAndSetBuffer(ref agentBuffer, agents, compute, "agents", updateKernel);
            
            // Set the variables in the Compute Shaders
            compute.SetInt("numAgents", settings.numAgents);
            drawAgentsCS.SetBuffer(0, "agents", agentBuffer);
            drawAgentsCS.SetInt("numAgents", settings.numAgents);

            compute.SetInt("width", settings.width);
            compute.SetInt("height", settings.height);
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < settings.stepsPerFrame; i++)
            {
                RunSimulation();
            }
        }

        private void LateUpdate()
        {
            if (showAgentsOnly)
            {
                ComputeHelper.ClearRenderTexture(displayTexture);

                drawAgentsCS.SetTexture(0, "TargetTexture", displayTexture);
                ComputeHelper.Dispatch(drawAgentsCS, settings.numAgents, 1, 1, 0);

            }
            else
            {
                ComputeHelper.Dispatch(compute, settings.width, settings.height, 1, kernelIndex: colourKernel);
                //	ComputeHelper.CopyRenderTexture(trailMap, displayTexture);
            }
        }

        private void RunSimulation()
        {
            var speciesSettings = settings.speciesSettings;

            ComputeHelper.CreateStructuredBuffer(ref settingsBuffer, speciesSettings);
            compute.SetBuffer(updateKernel, "speciesSettings", settingsBuffer);
            compute.SetBuffer(colourKernel, "speciesSettings", settingsBuffer);

            // Assign settings
            compute.SetFloat("deltaTime", Time.fixedDeltaTime);
            compute.SetFloat("time", Time.fixedTime);

            compute.SetFloat("trailWeight", settings.trailWeight);
            compute.SetFloat("decayRate", settings.decayRate);
            compute.SetFloat("diffuseRate", settings.diffuseRate);
            compute.SetInt("numSpecies", speciesSettings.Length);

            ComputeHelper.Dispatch(compute, settings.numAgents, 1, 1, kernelIndex: updateKernel);
            ComputeHelper.Dispatch(compute, settings.width, settings.height, 1, kernelIndex: diffuseMapKernel);

            ComputeHelper.CopyRenderTexture(diffusedTrailMap, trailMap);
        }

        private void OnDestroy()
        {
            ComputeHelper.Release(agentBuffer, settingsBuffer);
        }
    }
}

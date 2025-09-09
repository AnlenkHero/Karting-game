using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kart.Project_Files.Shaders.Toon {
    struct LightSet {
        public int id;
        public Light light;
        public Vector3 dir;
        public Color color;
        public float atten;
        public float inView;

        public LightSet(Light newLight) {
            light = newLight;
            id = newLight.GetInstanceID();
            dir = Vector3.zero;
            color = Color.black;
            color.a = 0.01f;
            atten = 0f;
            inView = 1.1f; // Range -0.1 to 1.1 which is clamped 0-1 for faster consistent fade
        }
    }

    [ExecuteInEditMode]
    public class ToonHelper : MonoBehaviour
    {
        [SerializeField] Material material;
        [SerializeField] bool instanceMaterial;
        [SerializeField] Vector3 meshCenter = Vector3.zero;
        [SerializeField] int maxLights = 6;

        [Header("Receive Shadow Check")]
        [SerializeField] bool raycast = true;
        [SerializeField] LayerMask raycastMask = new LayerMask();
        [SerializeField] float raycastFadeSpeed = 10f;

        private Vector3 _posAbs;
        private Dictionary<int, LightSet> _lightSets;

        private Material _materialInstance;
        private SkinnedMeshRenderer _skinRenderer;
        private MeshRenderer _meshRenderer;

        void Start() 
        {
            Init();
            GetLights();
        }

        void OnValidate() 
        {
            Init();
            Update();
        }

        void Init() 
        {
            if (!material) return;
           /* if (instanceMaterial)
            {
                materialInstance = new Material(material);
                materialInstance.name = "Instance of " + material.name;
            } else 
            {*/
                _materialInstance = material;
           // }

          /*  skinRenderer = GetComponent<SkinnedMeshRenderer>();
            meshRenderer = GetComponent<MeshRenderer>();
            if (skinRenderer) skinRenderer.sharedMaterial = materialInstance;
            if (meshRenderer) meshRenderer.sharedMaterial = materialInstance;*/
        }

        // NOTE: If your game loads lights dynamically, this should be called to init new lights
        private void GetLights() 
        {
            _lightSets ??= new Dictionary<int, LightSet>();

            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            List<int> newIds = new List<int>();
            
            foreach (Light newLight in lights)
            {
                int id = newLight.GetInstanceID();
                newIds.Add(id);
                if (!_lightSets.ContainsKey(id)) 
                {
                    _lightSets.Add(id, new LightSet(newLight));
                }
            }
            
            List<int> oldIds = new List<int>(_lightSets.Keys);
            foreach (var id in oldIds.Where(id => !newIds.Contains(id)))
            {
                _lightSets.Remove(id);
            }
        }

        void Update() 
        {
            _posAbs = transform.position + meshCenter;
            
            if (Application.isEditor && !Application.isPlaying) 
            {
                GetLights();
            }

            UpdateMaterial();
        }

        void UpdateMaterial() 
        {
            if (!material) return;
            
            List<LightSet> sortedLights = new List<LightSet>();
            if (_lightSets != null)
            {
                sortedLights.AddRange(_lightSets.Values.Select(CalcLight));
            }
            
            sortedLights.Sort((x, y) => 
            {
                float yBrightness = y.color.grayscale * y.atten;
                float xBrightness = x.color.grayscale * x.atten;
                return yBrightness.CompareTo(xBrightness);
            });
            
            int i = 1;
            foreach (LightSet lightSet in sortedLights) 
            {
                if (i > maxLights) break;
                if (lightSet.atten <= Mathf.Epsilon) break;
                
                Color color = lightSet.color;
                color.a = Mathf.Clamp(lightSet.atten, 0.01f, 0.99f); 

                _materialInstance.SetVector($"_L{i}_dir", lightSet.dir.normalized);
                _materialInstance.SetColor($"_L{i}_color", color);
                i++;
            }
            
            while (i <= maxLights) 
            {
                _materialInstance.SetVector($"_L{i}_dir", Vector3.up);
                _materialInstance.SetColor($"_L{i}_color", Color.black);
                i++;
            }
            
            foreach (LightSet lightSet in sortedLights) 
            {
                _lightSets[lightSet.id] = lightSet;
            }
        }

        LightSet CalcLight(LightSet lightSet) 
        {
            Light light = lightSet.light;
            float inView = 1.1f;
            float dist;

            if (!light.isActiveAndEnabled) 
            {
                lightSet.atten = 0f;
                return lightSet;
            }

            switch (light.type) 
            {
                case LightType.Directional:
                    lightSet.dir = light.transform.forward * -1f;
                    inView = TestInView(lightSet.dir, 100f);
                    lightSet.color = light.color * light.intensity;
                    lightSet.atten = 1f;
                    break;

                case LightType.Point:
                    lightSet.dir = light.transform.position - _posAbs;
                    dist = Mathf.Clamp01(lightSet.dir.magnitude / light.range);
                    inView = TestInView(lightSet.dir, lightSet.dir.magnitude);
                    lightSet.atten = CalcAttenuation(dist);
                    lightSet.color = light.color * lightSet.atten * light.intensity * 0.1f;
                    break;

                case LightType.Spot:
                    lightSet.dir = light.transform.position - _posAbs;
                    dist = Mathf.Clamp01(lightSet.dir.magnitude / light.range);
                    float angle = Vector3.Angle(light.transform.forward * -1f, lightSet.dir.normalized);
                    float inFront = Mathf.Lerp(0f, 1f, (light.spotAngle - angle * 2f) / lightSet.dir.magnitude); // More edge fade when far away from light source
                    inView = inFront * TestInView(lightSet.dir, lightSet.dir.magnitude);
                    lightSet.atten = CalcAttenuation(dist);
                    lightSet.color = light.color * lightSet.atten * light.intensity * 0.05f;
                    break;

                default:
                    Debug.Log("Lighting type '" + light.type + "' not supported by Awesome Toon Helper (" + light.name + ").");
                    lightSet.atten = 0f;
                    break;
            }
            
            float fadeSpeed = (Application.isEditor && !Application.isPlaying)
                ? raycastFadeSpeed / 60f
                : raycastFadeSpeed * Time.deltaTime;

            lightSet.inView = Mathf.Lerp(lightSet.inView, inView, fadeSpeed);
            lightSet.color *= Mathf.Clamp01(lightSet.inView);

            return lightSet;
        }

        float TestInView(Vector3 dir, float dist) 
        {
            if (!raycast) return 1.1f;
            RaycastHit hit;
            if (Physics.Raycast(_posAbs, dir, out hit, dist, raycastMask)) 
            {
                Debug.DrawRay(_posAbs, dir.normalized * hit.distance, Color.red);
                return -0.1f;
            }

            Debug.DrawRay(_posAbs, dir.normalized * dist, Color.green);
            return 1.1f;
        }

        // Ref - Light Attenuation calc: https://forum.unity.com/threads/light-attentuation-equation.16006/#post-3354254
        float CalcAttenuation(float dist) 
        {
            return Mathf.Clamp01(1.0f / (1.0f + 25f * dist * dist) * Mathf.Clamp01((1f - dist) * 5f));
        }

        private void OnDrawGizmosSelected() 
        {
            Gizmos.DrawWireSphere(_posAbs, 0.1f);
        }
    }
}

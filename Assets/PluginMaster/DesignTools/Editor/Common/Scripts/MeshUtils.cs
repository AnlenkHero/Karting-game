/*
Copyright (c) 2020 Omar Duarte
Unauthorized copying of this file, via any medium is strictly prohibited.
Writen by Omar Duarte, 2020.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
using System.Linq;
using UnityEngine;

namespace PluginMaster
{
    public static class MeshUtils
    {
        public static bool IsPrimitive(Mesh mesh)
        {
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(mesh);
            return assetPath.Length < 6 ? false : assetPath.Substring(0, 6) != "Assets";
        }

        public static Collider AddCollider(Mesh mesh, GameObject target)
        {
            Collider collider = null;
            void AddMeshCollider()
            {
                var meshCollider = target.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = mesh;
                collider = meshCollider;
            }
            if (IsPrimitive(mesh))
            {
                if (mesh.name == "Sphere") collider = target.AddComponent<SphereCollider>();
                else if (mesh.name == "Capsule") collider = target.AddComponent<CapsuleCollider>();
                else if (mesh.name == "Cube") collider = target.AddComponent<BoxCollider>();
                else if (mesh.name == "Plane") AddMeshCollider();
            }
            else AddMeshCollider();
            return collider;
        }

        public static GameObject[] FindFilters(LayerMask mask, GameObject[] exclude = null, bool excludeColliders = true)
        {
            var objects = new System.Collections.Generic.HashSet<GameObject>();
#if UNITY_2022_2_OR_NEWER
            var meshFilters = GameObject.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None);
            var skinnedMeshes = GameObject.FindObjectsByType<SkinnedMeshRenderer>(FindObjectsSortMode.None);
#else
            var meshFilters = GameObject.FindObjectsOfType<MeshFilter>();
            var skinnedMeshes = GameObject.FindObjectsOfType<SkinnedMeshRenderer>();
#endif
            objects.UnionWith(meshFilters.Select(comp => comp.gameObject));
            objects.UnionWith(skinnedMeshes.Select(comp => comp.gameObject));

            var filterList = new System.Collections.Generic.List<GameObject>(objects);
            if (exclude != null)
            {
                filterList = new System.Collections.Generic.List<GameObject>(objects.Except(exclude));
                objects = new System.Collections.Generic.HashSet<GameObject>(filterList);
            }
            if (excludeColliders)
            {
#if UNITY_2022_2_OR_NEWER
                var colliders = GameObject.FindObjectsByType<Collider>(FindObjectsSortMode.None);
#else
                var colliders = GameObject.FindObjectsOfType<Collider>();
#endif
                var collidersSet
                    = new System.Collections.Generic.HashSet<GameObject>(colliders.Select(comp => comp.gameObject));
                filterList = new System.Collections.Generic.List<GameObject>(objects.Except(collidersSet));
            }
            filterList = filterList.Where(obj => (mask.value & (1 << obj.layer)) != 0).ToList();
            return filterList.ToArray();
        }

        public static bool Raycast(Ray ray, out RaycastHit hitInfo,
            out GameObject collider, GameObject[] filters, float maxDistance,
            bool sameOriginAsRay = true, Vector3 origin = new Vector3())
        {
            collider = null;
            hitInfo = new RaycastHit();
            hitInfo.distance = maxDistance;

            var minDistance = float.MaxValue;
            var resultHitNormal = Vector3.zero;
            var result = false;
            var hitPoint = Vector3.zero;
            var originPlane = new Plane(-ray.direction, origin);
            foreach (var filter in filters)
            {
                if (filter == null) continue;
                if (RayIntersectsGameObject(ray, filter, includeInactive: false,
                    out float hitDistance, out Vector3 hitNormal))
                {
                    hitPoint = ray.origin + ray.direction.normalized * hitDistance;
                    if (!sameOriginAsRay) hitDistance = originPlane.GetDistanceToPoint(hitPoint);
                    if (hitDistance > maxDistance) continue;
                    if (hitDistance > minDistance) continue;
                    result = true;
                    collider = filter;
                    minDistance = hitDistance;
                    resultHitNormal = hitNormal;
                }
            }
            if (result)
            {
                hitInfo.point = hitPoint;
                hitInfo.distance = minDistance;
                hitInfo.normal = resultHitNormal;
            }
            return result;
        }

        public static bool Raycast(Vector3 origin, Vector3 direction,
            out RaycastHit hitInfo, out GameObject collider, GameObject[] filters, float maxDistance)
        {
            var ray = new Ray(origin, direction);
            return Raycast(ray, out hitInfo, out collider, filters, maxDistance);
        }

        public static bool RaycastAll(Ray ray, out RaycastHit[] hitInfo,
            out GameObject[] colliders, GameObject[] filters, float maxDistance,
            bool sameOriginAsRay = true, Vector3 origin = new Vector3())
        {
            System.Collections.Generic.List<RaycastHit> hitInfoList = new System.Collections.Generic.List<RaycastHit>();
            System.Collections.Generic.List<GameObject> colliderList = new System.Collections.Generic.List<GameObject>();

            foreach (var filter in filters)
            {
                if (Raycast(ray, out RaycastHit hit, out GameObject collider, filters, maxDistance, sameOriginAsRay, origin))
                {
                    if (hit.distance > maxDistance) continue;
                    hitInfoList.Add(hit);
                    colliderList.Add(filter);
                }
            }
            hitInfo = hitInfoList.ToArray();
            colliders = colliderList.ToArray();
            return hitInfoList.Count > 0;
        }


        const string MeshRayIntersectComputeShaderPath = "Shaders/MeshRayIntersect";
        const string KernelName = "CSMain";
        static ComputeShader _meshRayIntersectComputeShader;

        public static bool RayIntersectsMesh(Ray ray, Mesh mesh, Transform meshTransform,
            out float distance, out Vector3 localNormal)
        {
            if (_meshRayIntersectComputeShader == null)
                _meshRayIntersectComputeShader = Resources.Load<ComputeShader>(MeshRayIntersectComputeShaderPath);
            var kernel = _meshRayIntersectComputeShader.FindKernel(KernelName);

            var localOrigin = meshTransform.InverseTransformPoint(ray.origin);
            var localDirection = meshTransform.InverseTransformDirection(ray.direction).normalized;

            var verts = mesh.vertices;
            var tris = mesh.triangles;
            var triCount = tris.Length / 3;

            var vertBuf = new ComputeBuffer(verts.Length, sizeof(float) * 3);
            var triBuf = new ComputeBuffer(tris.Length, sizeof(int));
            var distBuf = new ComputeBuffer(1, sizeof(uint));
            var normBuf = new ComputeBuffer(1, sizeof(float) * 3);

            vertBuf.SetData(verts);
            triBuf.SetData(tris);
            distBuf.SetData(new uint[] { 0x7F7FFFFFu });

            _meshRayIntersectComputeShader.SetBuffer(kernel, "vertices", vertBuf);
            _meshRayIntersectComputeShader.SetBuffer(kernel, "triangles", triBuf);
            _meshRayIntersectComputeShader.SetBuffer(kernel, "minDistanceBits", distBuf);
            _meshRayIntersectComputeShader.SetBuffer(kernel, "hitNormalBuffer", normBuf);
            _meshRayIntersectComputeShader.SetVector("rayOrigin", localOrigin);
            _meshRayIntersectComputeShader.SetVector("rayDirection", localDirection);

            var groups = Mathf.CeilToInt(triCount / 64f);
            _meshRayIntersectComputeShader.Dispatch(kernel, groups, 1, 1);

            var bits = new uint[1];
            distBuf.GetData(bits);

            Vector3[] nbuf = new Vector3[1];
            normBuf.GetData(nbuf);

            vertBuf.Release();
            triBuf.Release();
            distBuf.Release();
            normBuf.Release();

            distance = 0;
            localNormal = Vector3.zero;
            var localT = System.BitConverter.ToSingle(System.BitConverter.GetBytes((int)bits[0]), 0);
            if (localT == float.MaxValue) return false;

            var localHitPoint = localOrigin + localDirection * localT;
            var worldHitPoint = meshTransform.TransformPoint(localHitPoint);

            distance = Vector3.Distance(ray.origin, worldHitPoint);
            localNormal = nbuf[0];
            return true;
        }
        public static (Mesh mesh, Transform transform)[] GetAllMeshses(GameObject obj, bool includeInactive)
        {
            var result = new System.Collections.Generic.HashSet<(Mesh, Transform)>();
            var meshFilters = obj.GetComponentsInChildren<MeshFilter>(includeInactive);
            foreach (var mf in meshFilters)
            {
                if (mf.sharedMesh == null) continue;
                var renderer = mf.GetComponent<MeshRenderer>();
                if (!renderer.enabled) continue;
                result.Add((mf.sharedMesh, mf.transform));
            }
            var skinnedMeshRenderers = obj.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive);
            foreach (var smr in skinnedMeshRenderers)
            {
                if (smr.sharedMesh == null || !smr.enabled) continue;
                result.Add((smr.sharedMesh, smr.transform));
            }
            return result.ToArray();
        }
        public static bool RayIntersectsGameObject(Ray ray, GameObject gameObject, bool includeInactive,
            out float distance, out Vector3 hitNormal)
        {
            distance = float.MaxValue;
            hitNormal = Vector3.zero;
            var hitAny = false;
            var meshesAndTransforms = GetAllMeshses(gameObject, includeInactive);
            foreach (var mt in meshesAndTransforms)
            {
                if (mt.mesh == null) continue;
                if (RayIntersectsMesh(ray, mt.mesh, mt.transform, out float d, out Vector3 localN))
                {
                    hitAny = true;
                    distance = Mathf.Min(distance, d);
                    hitNormal = mt.transform.TransformDirection(localN).normalized;
                }
            }
            if (!hitAny)
            {
                distance = 0f;
                return false;
            }
            return true;
        }

    }
}

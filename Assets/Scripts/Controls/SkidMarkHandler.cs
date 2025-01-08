using UnityEngine;

namespace Kart.Controls
{
    public class SkidMarkHandler : MonoBehaviour
    {
        [SerializeField] private Transform skidMarkPrefab;
        [SerializeField] private KartController kart;
        [SerializeField] private WheelCollider[] wheelColliders;
        private readonly Transform[] skidMarks = new Transform[4];

        private void Update()
        {
            for (var i = 0; i < wheelColliders.Length; i++)
            {
                UpdateSkidMarks(i);
            }
        }

        private void UpdateSkidMarks(int i)
        {
            if (!kart.IsGrounded())
            {
                EndSkid(i);
                return;
            }

            if (kart.IsWheelDrifting(wheelColliders[i]))
            {
                StartSkid(i);
            }
            else
            {
                EndSkid(i);
            }
        }

        private void StartSkid(int i)
        {
            if (skidMarks[i] != null) return;
            skidMarks[i] = Instantiate(skidMarkPrefab, wheelColliders[i].transform);
            skidMarks[i].localPosition = -Vector3.up * (wheelColliders[i].radius * .9f);
            skidMarks[i].localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        private void EndSkid(int i)
        {
            if (skidMarks[i] == null) return;
            Transform holder = skidMarks[i];
            skidMarks[i] = null;
            holder.SetParent(null);
            holder.rotation = Quaternion.Euler(90f, 0f, 0f);
            Destroy(holder.gameObject, 5f);
        }
    }
}
using UnityEngine;
using VContainer.Unity;

namespace Inventory
{
    public interface IItem
    {
        public GameObject GameObject { get; set; }
        public void Activate();
        public void Disable();
        public LifetimeScope GetDropPrefab();
    }
}
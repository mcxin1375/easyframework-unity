/*----------------------------------------------------------------
// author:Cookie(mcx)
// date:2018/2/24
// describe:单个ab资源的对象池
//----------------------------------------------------------------*/

using UnityEngine;
using Object = UnityEngine.Object;

namespace EasyFramework
{
    public class ResPoolBehaviour : MonoBehaviour, IResRequest
    {
        public bool Alive { get; private set; }
        public string ResName => resName;
        public int PooledCount => transform.childCount;
        
        [SerializeField] private string resName;
        [SerializeField] private int createdCount;
        

        public static ResPoolBehaviour Create(string resName)
        {
            var behaviour = new GameObject(resName).AddComponent<ResPoolBehaviour>();
            behaviour.Initialize(resName);
            return behaviour;
        }
        private void Awake()
        {
            transform.SetParent(F.Behaviour.transform);
        }
        private void OnDestroy()
        {
            FDebug.Log($"ResPool OnDestroy: {ResName}");
        }

        private void Initialize(string name)
        {
            FDebug.Log($"ResPool OnCreate: {name}");
            resName = name;
            Alive = true;
            F.ResLoader.Load(ResName, this);
        }
        internal void Destroy()
        {
            Alive = false;
            Object.Destroy(gameObject);
        }
        public GameObject Rent(Transform parent)
        {
            if (transform.childCount > 0)
            {
                var c = transform.GetChild(0);
                c.SetParent(parent);
                c.gameObject.SetActive(true);
                return c.gameObject;
            }

            return CreateNew(parent);
        }
        public GameObject[] Rent(int count, Transform parent)
        {
            var gos = new GameObject[count];
            for (int i = 0; i < gos.Length; i++)
            {
                gos[i] = Rent(parent);
            }
            return gos;
        }
        public void PreCreate(int objCount)
        {
            for (int i = transform.childCount; i < objCount; i++)
            {
                GameObject go = CreateNew(transform);
                go.SetActive(false);
            }
        }
        public async ETask PreCreateAsync(int objCount)
        {
            if (transform.childCount >= objCount) return;
            
            var num = objCount - transform.childCount;
            var goArr = await F.ResLoader.CreateObjAsync(ResName, num, transform);
            if (goArr == null) return;
            
            // 由于利用transform.childCount来管理池，可能存在下面的组件还没有加上，对象已经被使用了

            createdCount += goArr.Length;
            foreach (var go in goArr)
            {
                var holder = go.AddComponent<ResBehaviour>();
                holder.PoolBehaviour = this;
                go.SetActive(false);
            }
        }

        private GameObject CreateNew(Transform parent = null)
        {
            GameObject go = F.ResLoader.CreateObj(ResName, parent);
            if (go == null) return null;
            
            createdCount++;
            
            var holder = go.AddComponent<ResBehaviour>();
            holder.PoolBehaviour = this;
            return go;
        }
    }
}
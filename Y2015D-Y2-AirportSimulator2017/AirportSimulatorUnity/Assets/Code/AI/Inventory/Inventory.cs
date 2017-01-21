using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace AI
{
    using GOAPv3;

    public class Inventory : MonoBehaviour
    {
        private float _money;
        /// <summary>
        /// In Cents.
        /// </summary>
        public float Money
        {
            get { return _money; }
            set
            {
                _money = Mathf.Clamp(value, 0.0f, float.PositiveInfinity);
            }
        }

        public string MoneyString { get { return string.Format("€ {0},-", (Money / 100.0f).ToString("F2")); } }

        private float _liquid;
        /// <summary>
        /// In Liters.
        /// </summary>
        public float Liquid
        {
            get { return _liquid; }
            set
            {
                _liquid = Mathf.Clamp(value, 0.0f, 3.0f);
            }
        }

        private float _food;
        /// <summary>
        /// In Kilograms.
        /// </summary>
        public float Food
        {
            get { return _food; }
            set
            {
                _food = Mathf.Clamp(value, 0.0f, 2.0f);
            }
        }

        private uint _lugage;
        /// <summary>
        /// In Cases.
        /// </summary>
        public uint Lugage
        {
            get { return _lugage; }
            set
            {
                _lugage = value;
            }
        }

        private bool _boardingPass;
        public bool BoardingPass
        {
            get { return _boardingPass; }
            set
            {
                _boardingPass = value;
            }
        }

        private GoapAgent _agent;
        
        private void Awake()
        {
            _agent = GetComponent<GoapAgent>();
            _money = Random.Range(1000.0f, 10000.0f);
            _liquid = Random.Range(0.0f, 1.0f);
            _food = Random.Range(0.0f, 1.0f);
            _boardingPass = false;
        }
    }
}

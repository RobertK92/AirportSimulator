using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AI
{
    using GOAPv3;

    [RequireComponent(typeof(QueueMember), typeof(UtilityAgent), typeof(Inventory))]
    public class AgentActionResults : MonoBehaviour
    {
        private QueueMember _queueMember;
        private UtilityAgent _uAgent;
        private Inventory _inventory;

        [Range(0.1f, 1.0f)]
        public float drinkAmountPerDrinkAction = 0.8f;

        [Range(0.1f, 1.0f)]
        public float eatAmountPerEatAction = 0.5f;

        [Range(0.1f, 1.0f)]
        public float foodPerPurchase = 1.0f;

        private void Awake()
        {
            _queueMember = GetComponent<QueueMember>();
            _uAgent = GetComponent<UtilityAgent>();
            _inventory = GetComponent<Inventory>();
        }

        private void Start()
        {
            foreach (GoapAction action in GetComponents<GoapAction>())
            {
                action.OnCompleted += OnActionCompleted;
            }
        }

        private void OnActionCompleted(System.Type actionType, GoapState state)
        {
            if(actionType == typeof(DrinkAction))
            {
                float drinkAmount = _inventory.Liquid < drinkAmountPerDrinkAction ? _inventory.Liquid : drinkAmountPerDrinkAction;
                _uAgent.Thirst -= drinkAmount;
                _uAgent.Toilet += drinkAmount / 3.0f;
                _inventory.Liquid -= drinkAmount;
                if (drinkAmount > 0.0f)
                    state.HasDrunk = true;
            }
            else if(actionType == typeof(EatAction))
            {
                float eatAmount = _inventory.Food < eatAmountPerEatAction ? _inventory.Food : eatAmountPerEatAction;
                _uAgent.Hunger -= eatAmount;
                _uAgent.Toilet += eatAmount / 7.0f;
                _inventory.Food -= eatAmount;
                if (eatAmount > 0.0f)
                    state.HasEaten = true;
            }
            else if(actionType == typeof(BuyFoodAction))
            {
                _inventory.Food += foodPerPurchase;
                _inventory.Money -= 900.0f;
            }
            else if(actionType == typeof(BuyDrinkAction))
            {
                _inventory.Liquid += foodPerPurchase;
                _inventory.Money -= 700.0f;
            }
			else if (actionType == typeof(ToiletAction))
			{
				_uAgent.Toilet = 0f;
				state.HasUsedToilet = true;
			}
            else if(actionType == typeof(CheckInAction))
            {
                _inventory.BoardingPass = true;
            }
            else if(actionType == typeof(BuySnackAction))
            {
                _inventory.Food += foodPerPurchase;
                _inventory.Liquid += foodPerPurchase;
                _inventory.Money -= 1000.0f;
            }
		}
    }
}

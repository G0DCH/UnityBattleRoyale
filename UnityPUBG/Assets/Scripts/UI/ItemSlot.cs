using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityPUBG.Scripts.Entities;
using UnityPUBG.Scripts.Items;
using UnityPUBG.Scripts.Logic;

namespace UnityPUBG.Scripts.UI
{
    public class ItemSlot : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public int slotIndex;

        [Header("Background")]
        public Image backgroundImage;
        public Sprite defaultBackGroundSprite;
        public Sprite quickSlotBackgroundSprite;

        [Header("Slot")]
        public Image slotImage;
        public Sprite defaultSlotSprite;
        public TMPro.TextMeshProUGUI itemCountText;

        /// <summary>
        /// 빈 슬롯 이미지
        /// </summary>
        [SerializeField]
        private Sprite emptySlotImage;

        /// <summary>
        /// 현재 슬롯 이미지
        /// </summary>
        private Image slotImage = null;


        public bool Available
        {
            graphicRaycaster = transform.root.GetComponent<GraphicRaycaster>();
            siblingIndex = transform.GetSiblingIndex();
            planeDistance = transform.root.GetComponent<Canvas>().planeDistance;

            slotImage = transform.GetChild(0).GetComponent<Image>();

            //Debug.Log(slotImage.gameObject);
        }

        #region Unity 콜백
        private void Awake()
        {
            backgroundImage.sprite = defaultBackGroundSprite;
            slotImage.sprite = defaultSlotSprite;

            Available = false;
            graphicRaycaster = transform.root.GetComponent<GraphicRaycaster>();
        }

        private void OnEnable()
        {
            UpdateSlotObject();
        }
        #endregion

        public void UpdateSlotObject()
        {
            if (Available == false)
            {
                return;
            }

            Player targetPlayer = EntityManager.Instance.MyPlayer;
            Item itemAtSlot = targetPlayer.ItemContainer.GetItemAt(slotIndex);

            if (itemAtSlot.IsStackEmpty)
            {
                slotImage.sprite = defaultSlotSprite;
                backgroundImage.sprite = defaultBackGroundSprite;
                itemCountText.text = string.Empty;
            }
            else
            {
                slotImage.sprite = item.Data.Icon;
            }
        }

        #region Drag 핸들러
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (EntityManager.Instance.MyPlayer.ItemContainer.Count < slotIndex + 1)
            {
                isDrag = false;
                return;
            }

            originPosition = transform.position;
            isDrag = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isDrag)
            {
                transform.position = eventData.position;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (isDrag)
            {
                transform.position = originPosition;

                PointerEventData pointerEventData = new PointerEventData(GetComponent<EventSystem>());
#if !UNITY_ANDRIOD
                pointerEventData.position = Input.mousePosition;
#else
                pointerEventData.position = Input.touches[0].position;
#endif
                List<RaycastResult> results = new List<RaycastResult>();
                graphicRaycaster.Raycast(pointerEventData, results);
                isDrag = false;

                if (results.Count > 0)
                {
                    Item itemAtSlot = EntityManager.Instance.MyPlayer.ItemContainer.GetItemAt(slotIndex);

                    //쓰레기 통에 넣은 경우
                    if (results[0].gameObject.name == "TrashCanBackground")
                    {
                        EntityManager.Instance.MyPlayer.DropItemsAtSlot(slotIndex, itemAtSlot.CurrentStack);
                    }
                    //퀵슬롯에 넣은 경우
                    else if (results[0].gameObject.GetComponent<QuickItemSlot>() != null)
                    {
                        int quickSlotIndex = results[0].gameObject.GetComponent<QuickItemSlot>().quickSlotIndex;
                        EntityManager.Instance.MyPlayer.AssignItemToQuickBar(quickSlotIndex, itemAtSlot);
                    }
                }
            }
        }
        #endregion

        public void OnPointerClick(PointerEventData eventData)
        {
            float currentTimeClick = eventData.clickTime;
            if (Mathf.Abs(currentTimeClick - lastTouchTime) < 0.75f)
            {
                Player myPlayer = EntityManager.Instance.MyPlayer;
                Item itemAtSlot = myPlayer.ItemContainer.GetItemAt(slotIndex);

                if(itemAtSlot.Data is ConsumableData)
                {
                    myPlayer.UseItemAtItemContainer(slotIndex);
                }
            }
            lastTouchTime = currentTimeClick;
        }
    }
}
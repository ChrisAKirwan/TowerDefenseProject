using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    private Animator animator;
    private CanvasGroup canvasGroup;
    public Menu[] children;
    [HideInInspector] public bool hasChildren { get; private set; }

    public bool isOpen
    {
        get { return animator.GetBool("isOpen"); }
        set { animator.SetBool("isOpen", value); }
    }

    public void Awake()
    {
        if (children.Length > 0)
            hasChildren = true;
        else
            hasChildren = false;

        animator = GetComponent<Animator>();
        canvasGroup = GetComponent<CanvasGroup>();

        //RectTransform rect = GetComponent<RectTransform>();
        //rect.offsetMax = rect.offsetMin = new Vector2(0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Open"))
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        else
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

        }
    }
}

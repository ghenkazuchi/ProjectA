using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseUnit : MonoBehaviour
{
	public int level;
	public Image unitImage;
	public abstract void SetUp();
}

using System;
using JetBrains.Annotations;
using Skaillz.SanityChecker.Attributes;
using UnityEngine;

public class SampleScript : MonoBehaviour
{
	[NotNull]
	public GameObject obj;

	[CheckInside]
	public TestClass cls;

	[NotNegative]
	public int x = 0;

	[GreaterThanOrEquals(5), LessThan(8)]
	public int y = 6;

	[SerializeField, NotNullOrEmpty] public string z;
	
	[Serializable]
	public class TestClass
	{
		[NotNull] public GameObject obj2;
	}
}

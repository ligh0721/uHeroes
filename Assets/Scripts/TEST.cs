using UnityEngine;
using System.Collections;
using System;
using LitJson;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

class Test
{
	public int? a;
	public int b;
}

static class STest
{
	public static void test<T> (this T skill) where T : Skill
	{
		Debug.Log (skill.GetType().Name);
		Debug.Log (typeof(T).Name);
	}
}

public class TEST : MonoBehaviour {
	void Awake () {
		Debug.Log ("===== TEST START =====");
		string json = "{\"b\": 3, \"pos\":{\"x\":1,\"y\":2}}";
		Test a = new Test();
		a.a = 1;
		a.b = 2;
		//a.pos = new Vector2 (3, 4);
		//json = JsonUtility.ToJson (a);
		//Test t = JsonMapper.ToObject<Test> (json);
		//Test t = JsonUtility.FromJson<Test>(json);

		Skill skill = new SplashPas("SplashAttack", 0.5f, new Coeff(0.75f, 0), 1f, new Coeff(0.25f, 0));
		skill.test ();
		//Debug.Log (t);
	}
}

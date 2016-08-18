using UnityEngine;
using System.Collections;

public static class ExtensionMethods{

	public static IEnumerator Move (this Transform t, Vector3 targetLocalPosition, float duration)
	{
		Vector3 diffVector = (targetLocalPosition - t.localPosition);
		float diffLength = diffVector.magnitude;
		diffVector.Normalize ();
		float counter = 0.0f;
		while (counter < duration) {
			float moveAmount = (Time.deltaTime * diffLength) / duration;
			t.localPosition += diffVector * moveAmount;

			counter += Time.deltaTime;
			yield return null;
		}
		t.position = targetLocalPosition;
	}

	public static IEnumerator Scale (this Transform t, Vector3 targetLocalScale, float duration)
	{

		Vector3 diffVector = (targetLocalScale - t.localScale);
		float diffLength = diffVector.magnitude;
		diffVector.Normalize ();

		float counter = 0;
		while (counter < duration) {
			float scaleAmount = (Time.deltaTime * diffLength) / duration;
			t.localScale += diffVector * scaleAmount;

			counter += Time.deltaTime;
			yield return null;
		}
		t.localScale = targetLocalScale;
	}
}

using System;
using Netologia.Quest.Characters;
using UnityEngine;
using UnityEngine.AI;

namespace Netologia.Quest.Characters
{
	// Поведение рабочего в фоновом режиме
	public class WorkerMovement : BaseMovement
	{//todo переместить всю логику в кэректера
		[Serializable]
		public struct WorkPoint
		{
			public Vector3 Position;
			public Interval StayInterval;
		}

		private float? _delay;
		private int _index;
		
		private Character _character;
		
		[SerializeField]
		private WorkPoint[] _points;

		protected override void Awake()
		{
			base.Awake();
			_character = GetComponent<Character>();
			
			if (_points.Length <= 1)
			{
				_agent.enabled = false;
				enabled = false;
			}
		}
		
		private void Update()
		{
			if (!TimeManager.IsGame) return;
			//Если бот взаимодействует с игроком - он никуда не идёт
			if (!_character.MoveAccess) return;

			//Movement state
			if (IsMove) return;

			var time = TimeManager.Time;
			//Arrived state
			if (!_delay.HasValue)
			{
				Debug.Log("Check: Arrived");
				_delay = time + _points[_index].StayInterval.Random;
			}
			//Change point stay
			if (_delay - time <= 0)//or _delay < time
			{
				_index = (_index + 1) % _points.Length;
				SetPosition(_points[_index].Position);
				
				_delay = null;
				Debug.Log("Check: Change point stay");
			}
		}
		
		private void OnDrawGizmos()
		{
			Gizmos.color = Color.green;

			if (_points.Length == 0) return;

			var prev = _points[0].Position;
			var size = Vector3.one * .4f;
			Gizmos.DrawCube(prev, size);

			for (int i = 1, iMax = _points.Length; i < iMax; i++)
			{
				var curr = _points[i].Position;
				Gizmos.DrawCube(curr, size);
				Gizmos.DrawLine(prev, curr);
				prev = curr;
			}
		}

#if UNITY_EDITOR
		[ContextMenu("Set Self Position")]
		private void SetSelfPosition()
			=> UnityEditor.ArrayUtility.Add(ref _points, new WorkPoint { Position = transform.position });

		[ContextMenu("Back To First Position")]
		private void BackToFirstPosition()
		{
			if(_points.Length == 0) return;
			transform.position = _points[0].Position;
		}

		[UnityEditor.CustomEditor(typeof(WorkerMovement))]
		private class Editor : Quest.Editor.OldCustomInspector { }
#endif
	}
}
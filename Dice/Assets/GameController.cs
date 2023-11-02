using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
   [SerializeField] private GameObject[] playerGameObjectArray;
   [SerializeField] private Transform[] startPointTransformArray;
   [SerializeField] private Transform[] wayPointTransformArray;
   [SerializeField] private Text[] dice1TextArray;
   [SerializeField] private Text[] dice2TextArray;
   
   [SerializeField] private Button resetButton;
   [SerializeField] private Button rollButton;

   [SerializeField] private Text currentPlayerTurnText;
   [SerializeField] private Text statusText;

   private int[] _characterWaypointIndexArray;
   private int _currentCharacterIndex;

   private int _currentTargetWaypointIndex;

   private bool _isBusy;

   private int _minRoll = 1;
   private int _maxRoll = 6;

   private int _rollCount = 20;
   private float _delayBetweenRoll = 0.075f;

   private float _moveSpeed = 10f;

   private bool _isMoveCharacter;

   private void Awake()
   {
      SetupButton();
      
      SetStatusText("non");
   }
   
   private void SetupButton()
   {
      resetButton.onClick.AddListener(ResetGame);
      rollButton.onClick.AddListener(RollDice);
   }
   
   private void ResetGame()
   {
      if (_isBusy) return;
      _isBusy = true;
      
      SetStatusText("Start Reset Game");
      
      ResetVariable();
      SetupDice();
      MovePlayerToStartPosition();
      
      SetStatusText("Finish Reset Game");

      _isBusy = false;
   }
   
   private void RollDice()
   {
      if (_isBusy) return;
      _isBusy = true;
      
      SetStatusText("Start Roll Dice");
      
      StartCoroutine(OnRollDice());
   }

   private IEnumerator OnRollDice()
   {
      var currentDice1Value = 0;
      var currentDice2Value = 0;
      
      for (var rollCountIndex = 0; rollCountIndex < _rollCount; rollCountIndex++)
      {
         currentDice1Value = Random.Range(_minRoll, _maxRoll + 1);
         currentDice2Value = Random.Range(_minRoll, _maxRoll + 1);

         dice1TextArray[_currentCharacterIndex].text = currentDice1Value.ToString();
         dice2TextArray[_currentCharacterIndex].text = currentDice2Value.ToString();
         
         yield return new WaitForSeconds(_delayBetweenRoll);
      }
      
      OnFinishRoll(currentDice1Value,currentDice2Value);
   }

   private void OnFinishRoll(int dice1Value,int dice2Value)
   {
      SetStatusText("Finish Roll Dice");
      
      var combineValue = dice1Value + dice2Value;
      
      MoveCharacter(combineValue);
   }

   private void MoveCharacter(int value)
   {
      SetStatusText("Start Move Character");
      
      _currentTargetWaypointIndex = _characterWaypointIndexArray[_currentCharacterIndex] + value;
      if (_currentTargetWaypointIndex >= wayPointTransformArray.Length)
         _currentTargetWaypointIndex = 0;
      
      _isMoveCharacter = true;
      
      MoveToNextWaypoint();
   }

   private void MoveToNextWaypoint()
   {
      if (_characterWaypointIndexArray[_currentCharacterIndex] >= wayPointTransformArray.Length)
      {
         _characterWaypointIndexArray[_currentCharacterIndex] = 0;
         _currentTargetWaypointIndex = 1;
      }
         
      StartCoroutine(OnMoveToNextWaypoint());
   }

   private IEnumerator OnMoveToNextWaypoint()
   {
      var startPosition = playerGameObjectArray[_currentCharacterIndex].transform.position;
      var endPosition = wayPointTransformArray[_characterWaypointIndexArray[_currentCharacterIndex]].position;

      var speed = _moveSpeed;

      var moveDuration = Vector3.Distance(startPosition, endPosition)/ speed;
      
      Debug.LogError("Current Distance : " + Vector3.Distance(startPosition, endPosition) + " Current Duration : " + moveDuration);
      
      yield return new WaitForSeconds(moveDuration);

      CheckWaypointIndex();
   }

   private void CheckWaypointIndex()
   {
      _characterWaypointIndexArray[_currentCharacterIndex]++;
      
      if(_characterWaypointIndexArray[_currentCharacterIndex] != _currentTargetWaypointIndex)
         MoveToNextWaypoint();
      else 
         OnTargetWaypoint();
   }

   private void OnTargetWaypoint()
   {
      _isMoveCharacter = false;
      
      OnFinishMoveCharacter();
   }

   private void OnFinishMoveCharacter()
   {
      SetStatusText("Finish Move Character");
      
      _currentCharacterIndex++;
      if (_currentCharacterIndex >= playerGameObjectArray.Length)
         _currentCharacterIndex = 0;
      
      for (var playerIndex = 0; playerIndex < playerGameObjectArray.Length; playerIndex++)
      {
         dice1TextArray[playerIndex].transform.parent.gameObject.SetActive(false);
         dice2TextArray[playerIndex].transform.parent.gameObject.SetActive(false);
      }
      
      dice1TextArray[_currentCharacterIndex].transform.parent.gameObject.SetActive(true);
      dice2TextArray[_currentCharacterIndex].transform.parent.gameObject.SetActive(true);
      
      currentPlayerTurnText.text = (_currentCharacterIndex + 1).ToString();
      
      _isBusy = false;
   }

   private void Update()
   {
      if(!_isMoveCharacter) return;
      
      var startPosition = playerGameObjectArray[_currentCharacterIndex].transform.position;
      var endPosition = wayPointTransformArray[_characterWaypointIndexArray[_currentCharacterIndex]].position;
      
      playerGameObjectArray[_currentCharacterIndex].transform.position =
         Vector3.MoveTowards(startPosition,endPosition,_moveSpeed*Time.deltaTime);
   }

   private void Start()
   {
      ResetGame();
   }
   
   private void ResetVariable()
   {
      _characterWaypointIndexArray = new int[playerGameObjectArray.Length];
      _currentCharacterIndex = 0;
      _currentTargetWaypointIndex = 0;

      currentPlayerTurnText.text = (_currentCharacterIndex + 1).ToString();
   }
   
   private void SetupDice()
   {
      for (var playerIndex = 0; playerIndex < playerGameObjectArray.Length; playerIndex++)
      {
         dice1TextArray[playerIndex].text = "0";
         dice2TextArray[playerIndex].text = "0";
         
         dice1TextArray[playerIndex].transform.parent.gameObject.SetActive(false);
         dice2TextArray[playerIndex].transform.parent.gameObject.SetActive(false);
      }
      
      dice1TextArray[0].transform.parent.gameObject.SetActive(true);
      dice2TextArray[0].transform.parent.gameObject.SetActive(true);
   }

   private void MovePlayerToStartPosition()
   {
      for (var playerIndex = 0; playerIndex < playerGameObjectArray.Length; playerIndex++)
      {
         playerGameObjectArray[playerIndex].transform.position = startPointTransformArray[playerIndex].position;
      }
   }

   private void SetStatusText(string text)
   {
      statusText.text = text;
   }
}

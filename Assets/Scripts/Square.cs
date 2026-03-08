using UnityEngine;
using System.Collections.Generic;

public class Square : MonoBehaviour
{
	[SerializeField] private int rotationSpeed;
	[SerializeField] private int eliminationStrokeLimit;
	[SerializeField] private float jumpForce;
	[SerializeField] private float checkGroundDistance;
	[SerializeField] private float checkWallDistance;
	[SerializeField] private float checkWallYOffset;
	[SerializeField] private float checkPlayerAboveDistance;
	[SerializeField] private float groundCheckTimerSize;
	[SerializeField] private float linearVelocityYTrigger;
	[SerializeField] private float linearVelocityXTrigger;
	[SerializeField] private float eliminationStrokeDurationSize;
	[SerializeField] private float cliffCheckDistance;
	[SerializeField] private float linearVelocityXToJumpTrigger;
	[SerializeField] private float platformMoveSpeed;
	[SerializeField] private LayerMask groundLayer;
	[SerializeField] private LayerMask platformLayer;
	private bool eliminationSoundPlay;
	private int eliminationStroke;
	private int jumpCtr;
	private int trianglesColliderListLength;
	private float groundCheckTimer;
	private float eliminationStrokeDuration;
	private AudioSource eliminationSound;
	private Rigidbody2D rigidbody2D;
	private Rigidbody2D targetRigidbody2D;
	private CircleCollider2D circleCollider2D;
	private Circle targetComponent;
	private GameObject paintObject;
	private GameObject explosionObject;
	private GameObject explosionReverseObject;
	private List<EdgeCollider2D> triangleCollidersList;
	public bool isDead;
	public bool onPause;
	public GameObject targetObject;
	public GameObject gridObject;

	private void Awake()
	{
		isDead = false;
		onPause = false;
		jumpCtr = 0;
		groundCheckTimer = 0f;
		rigidbody2D = GetComponent<Rigidbody2D>();
		circleCollider2D = GetComponent<CircleCollider2D>();
		paintObject = transform.Find("Paint").gameObject;
		explosionObject = transform.Find("Explosion").gameObject;
		explosionReverseObject = transform.Find("ExplosionReverse").gameObject;
	}

	private void Start()
	{
		trianglesColliderListLength = gridObject.GetComponent<SceneScript>().trianglesColliderList.Count;
		triangleCollidersList = gridObject.GetComponent<SceneScript>().trianglesColliderList;
		eliminationSound = gridObject.GetComponent<SceneScript>().soundsObject.transform.Find("EliminationSquare").GetComponent<AudioSource>();
	}

	private void Update()
	{
		if (IsPlatformed())
			transform.position += Vector3.down * platformMoveSpeed * Time.deltaTime;

		if (IsDamaged() || isDead)
		{
			GetComponent<Rigidbody2D>().simulated = false;
			isDead = true;
		}

		if (isDead)
			Elimination();

		if (targetObject != null && targetComponent == null)
		{
			targetComponent = targetObject.GetComponent<Circle>();
			targetRigidbody2D = targetComponent.GetComponent<Rigidbody2D>();
		}

		if (targetObject != null && !targetComponent.isDead)
			Move();

		if (targetComponent != null && !isDead && !targetComponent.isDead)
			rigidbody2D.simulated = targetRigidbody2D.simulated;
	}

	private void Move()
	{
		if (groundCheckTimer > 0)
			groundCheckTimer -= Time.deltaTime;

		if (IsGrounded() && groundCheckTimer <= 0)
			jumpCtr = 0;

		if (Direction() < 0 && !IsPlayerAbove())
			rigidbody2D.angularVelocity = rotationSpeed;

		if (Direction() > 0 && !IsPlayerAbove())
			rigidbody2D.angularVelocity = -rotationSpeed;

		if (IsPlayerAbove())
			rigidbody2D.angularVelocity = 0;

		if
		(
			(IsPlayerAbove() && IsGrounded() && rigidbody2D.linearVelocityX < linearVelocityXTrigger) ||
			(IsPlayerAbove() && !IsGrounded() && rigidbody2D.linearVelocityY < linearVelocityYTrigger) ||
			(Direction() != 0 && rigidbody2D.linearVelocityX < linearVelocityXTrigger && IsGrounded() && Direction() == IsWalled()) ||
			(Direction() != 0 && rigidbody2D.linearVelocityX < linearVelocityXTrigger && rigidbody2D.linearVelocityY < linearVelocityYTrigger && Direction() == IsWalled()) ||
			(Direction() != 0 && rigidbody2D.linearVelocityX >= linearVelocityXToJumpTrigger && IsGrounded() && Direction() == IsWalled()) ||
			(Direction() != 0 && rigidbody2D.linearVelocityX >= linearVelocityXToJumpTrigger && rigidbody2D.linearVelocityY < linearVelocityYTrigger && Direction() == IsWalled()) ||
			(IsCliffAhead() && Mathf.Abs(rigidbody2D.linearVelocityX) >= linearVelocityXToJumpTrigger)
		)
		{
			if ((jumpCtr == 0 && IsGrounded()) || (jumpCtr == 1 && rigidbody2D.linearVelocityY < linearVelocityYTrigger))
			{
				if (jumpCtr == 0)
					groundCheckTimer = groundCheckTimerSize;

				rigidbody2D.linearVelocity = new Vector2(rigidbody2D.linearVelocity.x, jumpForce);
				jumpCtr++;
			}
		}
	}

	private bool IsPlatformed()
	{
		return Physics2D.Raycast(
			transform.position,
			Vector2.down,
			checkGroundDistance,
			platformLayer
		);
	}

	public bool IsGrounded()
	{
		return Physics2D.Raycast(
			transform.position,
			Vector2.down,
			checkGroundDistance,
			groundLayer
		) || IsPlatformed();
	}

	private bool IsCliffAhead()
	{
		return (IsGrounded() &&
			!Physics2D.Raycast
				(
					new Vector3(transform.position.x + cliffCheckDistance, transform.position.y, transform.position.z),
					Vector2.down,
					checkGroundDistance,
					groundLayer
				) &&
			Direction() == 1)
			||
			(IsGrounded() &&
			!Physics2D.Raycast
				(
					new Vector3(transform.position.x - cliffCheckDistance, transform.position.y, transform.position.z),
					Vector2.down,
					checkGroundDistance,
					groundLayer
				) &&
			Direction() == -1);
	}

	private float IsWalled()
	{
		if (Physics2D.Raycast
				(
					new Vector3(transform.position.x, transform.position.y + checkWallYOffset, transform.position.z),
					Vector2.right,
					checkWallDistance,
					groundLayer
				))
			return 1;
		else if (Physics2D.Raycast
					(
						new Vector3(transform.position.x, transform.position.y + checkWallYOffset, transform.position.z),
						Vector2.left,
						checkWallDistance,
						groundLayer
					))
			return -1;
		else
			return 0;
	}

	private bool IsPlayerAbove()
	{
		if (targetObject != null)
			return Physics2D.Raycast
						(
							transform.position,
							Vector2.up,
							checkPlayerAboveDistance,
							1 << targetObject.layer
						);
		else
			return false;
	}

	private float Direction()
	{
		if (targetObject != null)
			return Mathf.Sign(targetObject.transform.position.x - transform.position.x);
		else
			return 0;
	}

	private void Elimination()
	{
		if (eliminationSoundPlay == false)
		{
			eliminationSound.Play();
			eliminationSoundPlay = true;
		}

		paintObject.SetActive(false);

		if (eliminationStrokeDuration >= eliminationStrokeDurationSize * eliminationStroke &&
			eliminationStroke < eliminationStrokeLimit)
		{

			if (eliminationStroke % 2 == 0)
			{
				explosionObject.SetActive(true);
				explosionReverseObject.SetActive(false);
			}
			else
			{
				explosionObject.SetActive(false);
				explosionReverseObject.SetActive(true);
			}

			eliminationStroke++;
		}

		if (eliminationStroke >= eliminationStrokeLimit)
		{
			gridObject.GetComponent<SceneScript>().squareList.Remove(gameObject);
			gridObject.GetComponent<SceneScript>().squareColliderList.Remove(gameObject.GetComponent<CircleCollider2D>());
			Destroy(gameObject);
		}

		eliminationStrokeDuration += Time.deltaTime;
	}

	private bool IsDamaged()
	{
		bool returnValue = false;
	
		if (trianglesColliderListLength > 0)
		{
			for (int i = 0; i < trianglesColliderListLength; i++)
				if (CollidersOverlapping(circleCollider2D, triangleCollidersList[i]))
					returnValue = true;
	
			return returnValue;
		}
		else
			return returnValue;
	}

	bool CollidersOverlapping(Collider2D col1, Collider2D col2)
	{
		return col1.bounds.Intersects(col2.bounds);
	}

}

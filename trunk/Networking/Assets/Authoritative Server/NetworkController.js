var targetController : ThirdPersonController;
private var jumpButton : boolean;
private var verticalInput : float;
private var horizontalInput : float;
private var lastJumpButton : boolean;
private var lastVerticalInput : float;
private var lastHorizontalInput : float;

function Update()
{
	// Sample user input	
	verticalInput = Input.GetAxisRaw("Vertical");
	horizontalInput = Input.GetAxisRaw("Horizontal");
	jumpButton = Input.GetButton("Jump");

	var tmpVal : int = 0;
	if (jumpButton) tmpVal = 1;

	if (verticalInput != lastVerticalInput || horizontalInput != lastHorizontalInput || lastJumpButton != jumpButton) {
		if (networkView != NetworkViewID.unassigned)
			networkView.RPC("SendUserInput", RPCMode.Server, horizontalInput, verticalInput, tmpVal);
	}
	
	lastJumpButton = jumpButton;
	lastVerticalInput = verticalInput;
	lastHorizontalInput = horizontalInput;	
}

@RPC
function SendUserInput(h : float, v : float, j : int)
{
	targetController.horizontalInput = h;
	targetController.verticalInput = v;
	if (j==1) targetController.jumpButton = true;
	else targetController.jumpButton = false;
}

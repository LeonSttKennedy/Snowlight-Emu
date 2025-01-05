<?php

if (!defined('IN_HK') || !IN_HK)
{
	exit;
}

if (HK_LOGGED_IN)
{
	exit;
}

if (isset($_POST['usr']) && isset($_POST['pwd']))
{
	$username = $_POST['usr'];
	$password = $_POST['pwd'];
	
	$sha1_password = strtolower($GetSecurity->Encrypt('SHA1', $password));
	$result = mysql_query("SELECT * FROM characters WHERE username = '$username' LIMIT 1");
	$resultfetch = mysql_fetch_array($result);
	
	$users = mysql_query("SELECT * FROM users WHERE id = '" . $resultfetch['account_uid'] . "' LIMIT 1");
	$uid = $GetUsers->Name2Id($username);
	$user_data = mysql_fetch_array($users);
	$no_rows = mysql_num_rows($result);
	if(empty($username) && empty($password) || empty($username) || empty($password))
	{
		$_SESSION['HK_LOGIN_ERROR'] = "You has to enter your credentials to login";
	}
	else
	{
		if ($no_rows >= 1) 
		{
			if($GetUsers->HasRight($uid, "hk_login"))
			{
				if($sha1_password == $user_data["account_password"])
				{
					session_destroy();
					session_start();
						
					$_SESSION['login'] = true;
					$_SESSION['hk_login'] = true;
					$_SESSION['id'] = $user_data['id'];
					$_SESSION['account_name'] = $resultfetch['username'];
					
					header("Location: http://" . hk_www . "/index.php?_cmd=main");
					exit;
				}
				else if($user_data["account_password"] != $sha1_password)
				{
					$_SESSION['HK_LOGIN_ERROR'] = 'Invalid details';
				}
			}
			else
			{
				$_SESSION['HK_LOGIN_ERROR'] = "You do not have permission to access this service";
			}
		}
		else if($no_rows <= 0)
		{
			$_SESSION['HK_LOGIN_ERROR'] = 'Invalid details';
		}
	}
}

?>
<html>
<head>
<title>uberHotel / housekeeping / login</title>
<style type="text/css">
body
{
	font-family: sans-serif;
	font-size: 75%;
	background: #FFFFFF;
	color: #000;
}

#text
{
	display: block;
	padding-top: 100px;
	padding-bottom: 10px;
	margin: 0 auto;
	text-align: right;
	width: 420px;
}

#loginblock
{
	display: block;
	margin: 10px auto;
	border: 1px solid #000;
	width: 400px;
	padding: 5px 15px 10px 15px;
}

#loginblock .info
{
	padding-bottom: 2px;
	margin-bottom: 5px;
}

input.biginput
{
	width: 100%;
	font-size: 2em;
	text-align: center;
	padding: 3px;
}
</style>
</head>
<body>

<div id="text">

	<img src="http://<?php echo hk_www; ?>/images/lock.png" style="vertical-align: middle;">&nbsp;
	<b>uberHotel Housekeeping</b> Login

</div>

<div id="loginblock">

	<div class="info">
				<p>
					This service is intended for staff only and monitored closely, with 24 hour IP Address records being taken. All activity
					is recorded, and abuse or unauthorized access will be dealt with appropriately.
				</p>
				
				<p>
					Your username and password to this area are personal. <i>Never</i> give them to anyone under
					<i>any</i> circumstances.
				</p>

				
				<p>
					Please provide proper authentication in order to access this service.
				</p>
	</div>

	<form method="post">

		<input type="text" name="usr" class="biginput" value="<?php if (LOGGED_IN) { echo USER_NAME; } ?>"><br />
		<br />
		<input type="password" name="pwd" class="biginput" value=""><br />
		<br />
		<input type="submit" class="biginput" value="Log in">
		<input type="button" onclick="document.location = '/';" class="biginput" value="Get me out of here">

	</form>
	
	<?php
	
	if (isset($_SESSION['HK_LOGIN_ERROR']))
	{
		echo '<b style="color: darkred;">' . $_SESSION['HK_LOGIN_ERROR'] . '</b>';
		unset($_SESSION['HK_LOGIN_ERROR']);
	}
	
	?>
	
				<?php if (LOGGED_IN) { ?>
				<p>
					You are currently logged in to the main site as <b><?php echo USER_NAME; ?></b>.
				</p>
				<?php } ?>	
	
</div>

</body>
</html>
<?php

if (!defined('IN_HK') || !IN_HK)
{
	exit;
}

if (!HK_LOGGED_IN || !$GetUsers->HasRight($GetUsers->Name2Id(USER_NAME), 'hotel_admin'))
{
	exit;
}

$regState = mysql_result(mysql_query("SELECT reg_status FROM site_config LIMIT 1"), 0);

if (isset($_GET['switch']))
{
	$newState = "1";

	if ($regState == "1")
	{
		$newState = "0";
	}
	
	mysql_query("UPDATE site_config SET reg_status = '" . $newState . "' LIMIT 1");
	$regState = $newState;
}

require_once "top.php";

?>			

<h1>Register State</h1>

<br />

<p>
	 Register state can be updated from here. 
</p>

<h2>
<?php

if ($regState == "1")
{
	echo '<span style="font-size: 120%; color: darkred;">Register state is currently CLOSED. Register is not accepting new users registration.</span><br /><input type="button" value="Open register" onclick="document.location = \'index.php?_cmd=reg&switch\';">';
}
else
{
	echo 'Our hotel are accepting new users registration.<br /><input type="button" value="Close register" onclick="document.location = \'index.php?_cmd=reg&switch\';" style="color: darkred; font-weight: bold;">';
}

?>
</h2>

<?php

require_once "bottom.php";

?>
<?php

if (!defined('IN_HK') || !IN_HK)
{
	exit;
}

if (!HK_LOGGED_IN || !$GetUsers->HasRight($GetUsers->Name2Id(USER_NAME), 'hotel_admin'))
{
	exit;
}

$popClient = '';

if (isset($_POST['username']))
{
	$username = $_POST['username'];
	$get = mysql_num_rows(mysql_query("SELECT * FROM characters WHERE username = '" . $username . "' LIMIT 1"));
	
	if ($get == 1)
	{
		$popClient = $username;
	}
	else
	{
		fMessage('error', 'User not found on database.');
	}
}

require_once "top.php";			

echo '<h1>External user sign on</h1>';

if ($get == 1 && $popClient != '')
{
	echo "<input type=\"button\" onclick=\"popSsoClient('" . $popClient . "'); window.location = 'index.php?_cmd=extsignon'\" value=\"Click here to log in as " . $username . "\" style=\"margin: 20px; font-size: 150%;\">";
	echo '<input type="button" value="Done" onclick="window.location = \'index.php?_cmd=extsignon\';">';
}
else
{
	echo '<br /><p>This tool allows hotel managament to sign in to the hotel with another account. This may be useful for high level moderation, debugging, and/or supporting users.</p><br />';
	echo '<form method="post">
	Username: <input type="text" name="username" value=""><input type="submit" value="Sign in">
	</form>';
}

require_once "bottom.php";

?>
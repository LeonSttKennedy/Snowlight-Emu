<?php

if (!defined('IN_HK') || !IN_HK)
{
	exit;
}

if (!HK_LOGGED_IN || !$GetUsers->HasRight($GetUsers->Name2Id(USER_NAME), 'hk_moderation'))
{
	exit;
}

$ip = '';

if (isset($_POST['ip'])) { $ip = $_POST['ip']; }

require_once "top.php";			

echo '<h1>IP Lookup / Clone Checker</h1>
<br /><p>This tool can be used for looking up a user\'s IP, particulary useful when you need to ban a person or computer rather than just accounts.</p>';

echo '<br />
<form method="post">
Username:<br />
<input type="text" name="user"><Br />
<input type="submit" value="Lookup">
</form>';

echo '<br />
<form method="post">
IP Address:<br />
<input type="text" name="ip"><Br />
<input type="submit" value="Lookup">
</form>';

if (isset($_POST['user']))
{
	$user = $_POST['user'];
	$get = mysql_query("SELECT last_ip FROM characters WHERE username = '" . $user . "' LIMIT 1");
	
	if (mysql_num_rows($get) > 0)
	{
		$ip = mysql_result($get, 0);
	}
	
	echo '<h2>' . $user . '\'s IP is<br /><b style="font-size: 150%;">' . $ip . '</b></h2>';
}

if (isset($ip) && strlen($ip) > 0)
{
	echo '<h2 style="font-size: 140%;">Users on this IP: ' . $ip . '</h2>';
	$get = mysql_query("SELECT * FROM characters WHERE last_ip = '" . $ip . "' LIMIT 50");
	
	while ($user = mysql_fetch_assoc($get))
	{
		$getuinfo = mysql_fetch_assoc(mysql_query("SELECT * FROM users WHERE id = '" . $user['account_uid'] . "' "));
		echo '<h2 style="width: 50%;"><B>' . $user['username'] . '</B> <Small>(ID: ' . $user['id'] . ')</small><br /><span style="font-weight: normal;">Last online: ' . date('d/m/Y H:i:s', $user['timestamp_lastvisit']) . '<br />E-mail: ' . $getuinfo['account_email'] . '<br />This user is <b>' . (($user['online'] == "1") ? 'online!' : 'offline') . '</b></span></h2>';
	}
}

require_once "bottom.php";

?>
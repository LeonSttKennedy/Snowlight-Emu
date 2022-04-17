<?php

if (!defined('IN_HK') || !IN_HK)
{
	exit;
}

if (!HK_LOGGED_IN || !$GetUsers->HasRight($GetUsers->Name2Id(USER_NAME), 'hotel_admin'))
{
	exit;
}

$data = null;
$u = 0;

if (isset($_GET['u']) && is_numeric($_GET['u']))
{
	$u = intval($_GET['u']);
	$getData = mysql_query("SELECT id,username FROM characters WHERE id = '" . $u . "' LIMIT 1");
	
	if (mysql_num_rows($getData) > 0)
	{
		$data = mysql_fetch_assoc($getData);
	}
}
else if (isset($_POST['usrsearch']))
{
	$usrSearch = $_POST['usrsearch'];
	$getData = mysql_query("SELECT id,username FROM characters WHERE username = '" . $usrSearch . "' LIMIT 1");
	
	if (mysql_num_rows($getData) > 0)
	{
		$data = mysql_fetch_assoc($getData);
		
		header("Location: index.php?_cmd=badges&u=" . $data['id']);
		exit;
	}	
	else
	{
		fMessage('error', 'User not found!');
	}
}

require_once "top.php";			

echo '<h1>Manage user badges</h1>';

if ($data == null)
{
	echo '<br />';
	echo '<p><i>No user set or invalid user supplied.</i> To edit an user\'s badges, search for one below..</p>';
	echo '<br />';
	echo '<p><form method="post">';
	echo 'By UID: <input id="uidval" type="text" size="5" name="uid">&nbsp; <input type="button" value="Go" onclick="window.location = \'index.php?_cmd=badges&u=\' + document.getElementById(\'uidval\').value;"><br />';
	echo 'By username: <input type="text" name="usrsearch" value="">&nbsp; <input type="submit" value="Go">';
	echo '</form></p>';
}
else
{
	if (isset($_GET['take']))
	{
		mysql_query("DELETE FROM badges WHERE user_id = '" . $data['id'] . "' AND badge_id = '" . $_GET['take'] . "'");
		
		if (mysql_affected_rows() >= 1)
		{
			echo '<b>Took badge ' . returnBadgeCode($_GET['take']) . ' from ' . $data['username'] . '.</b>';
		}
	}	
	
	if (isset($_POST['newbadge']))
	{
		$source_type = $_POST['source_type'];
		$badgeid = mysql_result(mysql_query("SELECT id FROM badge_definitions WHERE code = '" . $_POST['newbadge'] . "' LIMIT 1"), 0);
		if(intval($badgeid) > 0)
		{
			mysql_query("INSERT INTO `badges` (`id`, `user_id`, `badge_id`, `source_type`, `source_data`, `slot_id`) VALUES (NULL, '" . $data['id'] . "', '" . $badgeid . "' , '" . $source_type . "', '" . $_POST['newbadge'] . "','0')");
			echo '<b>Gave badge!</b>';
		}
		else
		{
			echo '<b><a href =\'index.php?_cmd=badgedefs\'>Click here</a> to check if the inserted badge has a definition in the database.</b>';
		}
	}
	echo '<br />';
	echo '<p>Try to not give user achievement badges from here, let the emulator take care of that itself.</p>';
	echo '<h2>Edting badges: ' . $data['username'] . ' (<a href="index.php?_cmd=badges">Back to user search</a>)</h2>';
	$getBadges = mysql_query("SELECT * FROM badges WHERE user_id = '" . $data['id'] . "'");
	
	echo '<Br /><table border="1">
	<thead>
	<tr>
		<td>Image</td>
		<td>Badge code</td>
		<td>Status</td>
		<td>Source Type</td>
		<td>Definition</td>
		<td>Controls</td>
	</tr>
	</thead>';
	
	while ($b = mysql_fetch_assoc($getBadges))
	{
		echo '<tr>';
		echo '<td><img src="http://192.168.15.72/cdn.classichabbo.com/r38/gordon/RELEASE63-34888-34886-201107192308_9e5b377e2ee4333b61eb9d20d356936d/c_images/album1584/' . returnBadgeCode($b['badge_id']) . '.gif"></td>';
		echo '<td><center>' . returnBadgeCode($b['badge_id']) . '</center></td>';
		echo '<td><center>';
		
		if ($b['slot_id'] == 0)
		{
			echo 'Not equipped';
		}
		else
		{
			echo 'Equipped in slot ' . $b['slot_id'];
		}
		
		echo '</center></td>';
		echo '<td><center>' . $b['source_type'] . '</center></td>';
		echo '<td><a href="index.php?_cmd=badgetexts">';
		$tryGet1 = mysql_query("SELECT sval FROM external_texts WHERE skey = 'badge_name_" . returnBadgeCode($b['badge_id']) . "'");
		$tryGet2 = mysql_query("SELECT sval FROM external_texts WHERE skey = 'badge_desc_" . returnBadgeCode($b['badge_id']) . "'");
		
		if (mysql_num_rows($tryGet1) > 0)
		{
			echo '<b>' . mysql_result($tryGet1, 0) . '</b><br />';
		}
		else
		{
			echo '<b><i>(No name def)</i></b><br />';
		}
		
		if (mysql_num_rows($tryGet2) > 0)
		{
			echo mysql_result($tryGet2, 0);
		}
		else
		{
			echo '<i>(No descr def)</i><br />';
		}		
		
		echo '</a></td>';
		echo '<td><center><input type="button" onclick="window.location = \'index.php?_cmd=badges&u=' . $u . '&take=' . $b['badge_id'] . '\';" value="Take"></center></td>';
		echo '</tr>';
	}
	
	echo '<tr><form method="post">
	<td><center>?</center></td>
	<td><input type="text" name="newbadge" value="" style="padding: 5px; font-size: 130%; text-align: center;"></td>
	<td><center>(New badge)</center></td>
	<td><center><select name="source_type" style="padding: 5px; font-size: 100%; text-align: center;"><option value="static">static</option><option value="achievement">achievement</option></select></center></td>
	<td>&nbsp;</td>
	<td><center><input type="submit" value="Give" onclick=""></center></td>
	</form></tr>';
}

require_once "bottom.php";

?>
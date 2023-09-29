<?php

if (!defined('IN_HK') || !IN_HK)
{
	exit;
}

if (!HK_LOGGED_IN || !$GetUsers->HasRight($GetUsers->Name2Id(USER_NAME), 'hotel_admin'))
{
	exit;
}

if (isset($_POST['edit-no']))
{
	mysql_query("UPDATE external_texts SET sval = '" . $_POST['nm'] . "' WHERE skey = 'badge_name_" . $_POST['edit-no'] . "' LIMIT 1");
	mysql_query("UPDATE external_texts SET sval = '" . $_POST['dc'] . "' WHERE skey = 'badge_desc_" . $_POST['edit-no'] . "' LIMIT 1");
	
	fMessage('ok', 'Updated badge texts.');
}

if (isset($_POST['newbadge']))
{
	mysql_query("INSERT INTO external_texts (skey,sval) VALUES ('badge_name_" . $_POST['newbadge'] . "','" . $_POST['newname']. "')");
	mysql_query("INSERT INTO external_texts (skey,sval) VALUES ('badge_desc_" . $_POST['newbadge'] . "','" . $_POST['newdescr']. "')");
	fMessage('ok', 'Badge texts inserted.');
}

if (isset($_GET['doDel']))
{
	mysql_query("DELETE FROM external_texts WHERE skey = 'badge_name_" . $_GET['doDel'] . "' LIMIT 1");
	mysql_query("DELETE FROM external_texts WHERE skey = 'badge_desc_" . $_GET['doDel'] . "' LIMIT 1");
	
	fMessage('ok', 'Badge texts removed.');
	
	header("Location: index.php?_cmd=badgetexts");
	exit;
}

require_once "top.php";

echo '<h1>Badge text definitions</h1>';
echo '<p>This tool can be used to manage badge text definitions (the text that appears).</p><br />';

echo '<table border="1" width="100%">';
echo '<thead>';
echo '<tr>';
echo '<td>Badge</td>';
echo '<td>Name</td>';
echo '<td>Description</td>';
echo '<td>Controls</td>';
echo '</tr>';

$get = mysql_query("SELECT * FROM external_texts WHERE skey LIKE '%badge_name_%'");

while ($text = mysql_fetch_assoc($get))
{
	$badgeName = substr($text['skey'], 11);
	$badgeTName = $text['sval'];
	$badgeTDescr = mysql_result(mysql_query("SELECT sval FROM external_texts WHERE skey = 'badge_desc_" . $badgeName . "' LIMIT 1"), 0);

	echo '<tr><form method="post">';
	echo '<td><img src="' . CLIENT_BASE . '/c_images/album1584/' . $badgeName . '.gif" style="vertical-align: middle;">&nbsp;&nbsp;' . $badgeName . '</td>';
	echo '<input type="hidden" name="edit-no" value="' . $badgeName . '">';
	echo '<td><input type="text" style="width: 100%; padding: 10px; font-size: 115%;" name="nm" value="' . $badgeTName . '"></td>';
	echo '<td><textarea style="width: 100%; height: 100%;" name="dc">' . $badgeTDescr . '</textarea></td>';
	echo '<td><center><input type="submit" value="Update">&nbsp;<input type="button" value="Delete" onclick="window.location = \'index.php?_cmd=badgetexts&doDel=' . $badgeName . '\';"></center></td>';
	echo '</form></tr>';
}

echo '<tr><form method="post">';
echo '<td><input type="text" style="width: 100%; padding: 10px; font-size: 115%;" name="newbadge"></td>';
echo '<td><input type="text" style="width: 100%; padding: 10px; font-size: 115%;" name="newname"></td>';
echo '<td><textarea name="newdescr" style="width: 100%; height: 100%;"></textarea>';
echo '<td><center><input type="submit" value="Add"></center>';
echo '</form></tr>';

echo '</thead>';
echo '</table>';

require_once "bottom.php";

?>
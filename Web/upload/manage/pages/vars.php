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
	mysql_query("UPDATE external_variables SET skey = '" . $_POST['key'] . "', sval = '" . $_POST['value'] . "' WHERE skey = '" . $_POST['edit-no'] . "' LIMIT 1");
}

if (isset($_POST['newkey']))
{
	mysql_query("INSERT INTO external_variables (skey,sval) VALUES ('" . $_POST['newkey'] . "','" . $_POST['newval'] . "')");
}

if (isset($_GET['doDel']))
{
	mysql_query("DELETE FROM external_variables WHERE skey = '" . $_GET['doDel'] . "' LIMIT 1");
	fMessage('ok', 'Key removed.');
	header("Location: index.php?_cmd=vars");
	exit;
}

require_once "top.php";

echo '<h1>External variables (overrides)</h1>';
echo '<p>This tool can be used to override external variables keys.</p><br />';

echo '<a href="http://192.168.15.72/cdn.classichabbo.com/r38/gordon/RELEASE63-34888-34886-201107192308_9e5b377e2ee4333b61eb9d20d356936d/external_variables.txt" id="">Current external variables</a>';

echo '<table border="1" width="100%">';
echo '<thead>';
echo '<tr>';
echo '<td>Key</td>';
echo '<td>Value</td>';
echo '<td>Controls</td>';
echo '</tr>';

$get = mysql_query("SELECT * FROM external_variables");

while ($text = mysql_fetch_assoc($get))
{
	echo '<tr><form method="post">';
	echo '<input type="hidden" name="edit-no" value="' . $text['skey'] . '">';
	echo '<td><input type="text" style="width: 100%; padding: 10px; font-size: 115%;" name="key" value="' . $text['skey'] . '"></td>';
	echo '<td><textarea style="width: 100%; height: 100%;" name="value">' . $text['sval'] . '</textarea></td>';
	echo '<td><center><input type="submit" value="Update">&nbsp;<input type="button" value="Delete" onclick="window.location = \'index.php?_cmd=vars&doDel=' . $text['skey'] . '\';"></center></td>';
	echo '</form></tr>';
}

echo '<tr><form method="post">';
echo '<td><input type="text" style="width: 100%; padding: 10px; font-size: 115%;" name="newkey"></td>';
echo '<td><textarea name="newval" style="width: 100%; height: 100%;"></textarea>';
echo '<td><center><input type="submit" value="Add">';
echo '</form></tr>';

echo '</thead>';
echo '</table>';

require_once "bottom.php";

?>
<?php

if (!defined('IN_HK') || !IN_HK)
{
	exit;
}

if (!HK_LOGGED_IN || !$GetUsers->HasRight($GetUsers->Name2Id(USER_NAME), 'hotel_admin'))
{
	exit;
}

$limit = 14;
$page = ((isset($_GET['page'])) ? $_GET['page'] : 1);

$start = $page - 1;
$start = $start * $limit;

if (isset($_POST['edit-no']))
{
	mysql_query("UPDATE server_ingame_texts SET display_text = '" . $_POST['value'] . "' WHERE identifier = '" . $_POST['edit-no'] . "' LIMIT 1");
}

if (isset($_POST['newidentifier']))
{
	mysql_query("INSERT INTO server_ingame_texts (identifier, display_text) VALUES ('" . $_POST['newidentifier'] . "','" . $_POST['newdisplaytext'] . "')");
}

if (isset($_GET['doDel']))
{
	mysql_query("DELETE FROM server_ingame_texts WHERE identifier = '" . $_GET['doDel'] . "' LIMIT 1");
	fMessage('ok', 'Key removed.');
	header("Location: index.php?_cmd=servertexts");
	exit;
}

require_once "top.php";

echo '<h1>In-game server texts</h1>';
echo '<p>This tool can be used to update in-game server  texts or translate to your language.</p><br />';

echo '<div style="float: left; width: 67%;">';
echo '<h2>Current server texts</h2>';
echo '<br />';
echo '<table border="1" width="100%">';
echo '<thead>';
echo '<tr>';
echo '<td>Key</td>';
echo '<td>Value</td>';
echo '<td>Controls</td>';
echo '</tr>';

$get = mysql_query("SELECT * FROM server_ingame_texts LIMIT $start,$limit");
$all = intval(mysql_num_rows(mysql_query("SELECT * FROM server_ingame_texts")));

while ($text = mysql_fetch_assoc($get))
{
	echo '<tr><form method="post">';
	echo '<input type="hidden" name="edit-no" value="' . $text['identifier'] . '">';
	echo '<td>' . $text['identifier'] . '</td>';
	echo '<td><textarea style="width: 100%; height: 100%;" name="value">' . $text['display_text'] . '</textarea></td>';
	echo '<td><center><input type="submit" value="Update">&nbsp;<input type="button" value="Delete" onclick="window.location = \'index.php?_cmd=servertexts&doDel=' . $text['identifier'] . '\';"></center></td>';
	echo '</form></tr>';
}

$tp = ceil($all / $limit);

$pagesstring = '';
$prev = $page -1;
$next = $page +1;

if ($page > 1) 
{
	$pagesstring .= "<a href='index.php?_cmd=servertexts&page=1'>First Page</a> <a href='index.php?_cmd=servertexts&page=$prev'>&laquo; Previous</a> ";
}
$rightindex = 3;
$leftindex = ($page > 2) ? 2 : $page - 1;
for($i = $page - $leftindex; $i < $page + $rightindex; $i++)
{
	if($page == $i)
	{
		$pagesstring .= ' <b>' . $i . '</b>';
	}
	else
	{
		if($i >= 1 && $i <= $tp)
		{
			$pagesstring .= " <a href='index.php?_cmd=servertexts&page=$i'>" . $i . "</a>";
		}
	}
}

if ($page < $tp) 
{
	$pagesstring .= " <a href='index.php?_cmd=servertexts&page=$next'>Next &raquo;</a> <a href='index.php?_cmd=servertexts&page=$tp'>Last Page</a>";
}

if($page == $tp)
{
	echo '<tr><form method="post">';
	echo '<td><input type="text" style="width: 100%; padding: 10px; font-size: 115%;" name="newidentifier"></td>';
	echo '<td><textarea name="newdisplaytext" style="width: 100%; height: 100%;"></textarea>';
	echo '<td><center><input type="submit" value="Add">';
	echo '</form></tr>';
}

echo '</thead>';
echo '</table>';
echo '<center style="width: 100%; padding: 10px; font-size: 115%;">' . $pagesstring . '</center>';
echo '</div>';
echo '<div style="float: right; width: 31%;">';
echo '<h2>Notes</h2>';
echo '<br />';
echo '<ol style="padding-left: 15px;" type="a">';
echo '<li>To add a new text go to last page, and check if this are in use on server.</li>';
echo '<li>You can use "\n" or "\r" to add a new line in the warnings</li>';
echo '<li><p style="color: darkred;">Do not change the variables within the % (Similar to: %any number%)</p></li>';
echo '</ol>';
echo '</div>';
echo '<div style="clear: both;"></div>';

require_once "bottom.php";

?>
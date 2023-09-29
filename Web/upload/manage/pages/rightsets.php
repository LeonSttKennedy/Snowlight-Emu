<?php

if (!defined('IN_HK') || !IN_HK)
{
	exit;
}

if (!HK_LOGGED_IN || !$GetUsers->HasRight($GetUsers->Name2Id(USER_NAME), 'hotel_admin'))
{
	exit;
}

$limit = 10;
$page = ((isset($_GET['page'])) ? $_GET['page'] : 1);

$start = $page - 1;
$start = $start * $limit;

if (isset($_POST['edit-no']))
{
	mysql_query("UPDATE rights SET set_id = '" . $_POST['setid'] . "', right_id = '" . $_POST['rightid'] . "' WHERE id = '" . $_POST['edit-no'] . "' LIMIT 1");
	
	fMessage('ok', 'Updated rights sets.');
}

if (isset($_POST['newsetid']))
{
	mysql_query("INSERT INTO rights (id, set_id, right_id) VALUES (NULL, '" . $_POST['newsetid'] . "','" . $_POST['newrightid']. "')");
	fMessage('ok', 'Right set inserted.');
}

if (isset($_GET['doDel']))
{
	mysql_query("DELETE FROM rights WHERE id = '" . $_GET['doDel'] . "' LIMIT 1");
	
	fMessage('ok', 'Right sets removed.');
	
	header("Location: index.php?_cmd=rightsets");
	exit;
}

require_once "top.php";

echo '<h1>Right sets definitions</h1>';
echo '<p>This tool can be used to manage right sets definitions (the rights of badges).</p><br />';

echo '<table border="1" width="100%">';
echo '<thead>';
echo '<tr>';
echo '<td>ID</td>';
echo '<td>Set ID</td>';
echo '<td>Right ID</td>';
echo '<td>Controls</td>';
echo '</tr>';

$get = mysql_query("SELECT * FROM rights LIMIT $start,$limit");
$all = intval(mysql_num_rows(mysql_query("SELECT * FROM rights")));
$total = intval(mysql_result(mysql_query("SELECT id FROM rights ORDER BY id DESC LIMIT 1"), 0)) + 1;

while ($text = mysql_fetch_assoc($get))
{
	$rightID = $text['id'];
	$rightSetID = $text['set_id'];
	$rightRightID = $text['right_id'];

	echo '<tr><form method="post">';
	echo '<td>#' . $rightID . '</td>';
	echo '<input type="hidden" name="edit-no" value="' . $rightID . '" />';
	echo '<td><input type="text" style="width: 100%; padding: 10px; font-size: 115%;" name="setid" value="' . $rightSetID . '" /></td>';
	echo '<td><input type="text" style="width: 100%; padding: 10px; font-size: 115%;" name="rightid" value="' . $rightRightID . '" /></td>';
	echo '<td><center><input type="submit" value="Update">&nbsp;<input type="button" value="Delete" onclick="window.location = \'index.php?_cmd=rightsets&doDel=' . $rightID . '\';"></center></td>';
	echo '</form></tr>';
}

$tp = ceil($all / $limit);

if($page == $tp)
{
	echo '<tr><form method="post">';
	echo '<td>#' . $total . '</td>';
	echo '<td><input type="text" style="width: 100%; padding: 10px; font-size: 115%;" name="newsetid"></td>';
	echo '<td><input type="text" style="width: 100%; padding: 10px; font-size: 115%;" name="newrightid"></input></td>';
	echo '<td><center><input type="submit" value="Add"></td>';
	echo '</form></tr>';
}

echo '</thead>';
echo '</table>';

$pagesstring = '';
$prev = $page -1;
$next = $page +1;

if ($page > 1) 
{
	$pagesstring .= "<li><a href='index.php?_cmd=rightsets&page=1'>First Page</a></li>";
	$pagesstring .= "<li><a href='index.php?_cmd=rightsets&page=$prev'>&laquo;</a></li>";
}
$rightindex = 3;
$leftindex = ($page > 2) ? 2 : $page - 1;
for($i = $page - $leftindex; $i < $page + $rightindex; $i++)
{
	if($i >= 1 && $i <= $tp)
	{
		$pagesstring .= "<li><a" . ($page == $i? ' class="active" ' : '' ) . " href='index.php?_cmd=rightsets&page=$i'>$i</a></li>";
	}
}

if ($page < $tp) 
{
	$pagesstring .= "<li><a href='index.php?_cmd=rightsets&page=$next'>&raquo;</a></li>";
	$pagesstring .= "<li><a href='index.php?_cmd=rightsets&page=$tp'>Last Page</a></li>";
}

echo '<div class="center"><ul class="pagination">' . $pagesstring . '</ul></div>';

require_once "bottom.php";
?>
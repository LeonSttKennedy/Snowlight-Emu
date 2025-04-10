<?php

if (!defined('IN_HK') || !IN_HK)
{
	exit;
}

if (!HK_LOGGED_IN || !$GetUsers->HasRight($GetUsers->Name2Id(USER_NAME), 'hotel_admin'))
{
	exit;
}

$limit = 15;
$page = ((isset($_GET['page'])) ? $_GET['page'] : 1);

$start = $page - 1;
$start = $start * $limit;

if (isset($_POST['edit-name']))
{
	mysql_query("UPDATE badge_definitions SET rights_sets = '" . $_POST['rightsets'] . "' WHERE code = '" . $_POST['edit-name'] . "' LIMIT 1");
	
	fMessage('ok', 'Updated badge def.');
}

if (isset($_POST['newbadge']))
{
	mysql_query("INSERT INTO badge_definitions (id, code, rights_sets) VALUES (NULL, '" . $_POST['newbadge'] . "', '" . $_POST['newbadgerightsets'] . "')");
	fMessage('ok', 'Badge definition inserted.');
}

if (isset($_GET['doDel']))
{
	mysql_query("DELETE FROM badge_definitions WHERE code = '". $_GET['doDel'] . "' LIMIT 1");
	
	fMessage('ok', 'Badge definition removed.');
	
	header("Location: index.php?_cmd=badgedefs");
	exit;
}

require_once "top.php";

echo '<h1>Badge definitions</h1>';
echo '<p>This tool can be used to manage badge definitions (To give user rights).</p>';
echo '<p>Can be used a \',\' to separe multiple rights IDs.</p>';

echo '<div style="float: left; width: 67%;">';
echo '<h2>Badge List</h2>';
echo '<br />';
echo '<table border="1" width="100%">';
echo '<thead>';
echo '<tr>';
echo '<td>ID</td>';
echo '<td>Badge</td>';
echo '<td>Right Sets</td>';
echo '<td>Controls</td>';
echo '</tr>';

$get = mysql_query("SELECT * FROM badge_definitions LIMIT $start,$limit");
$all = intval(mysql_num_rows(mysql_query("SELECT * FROM badge_definitions")));
$total = intval(mysql_result(mysql_query("SELECT id FROM badge_definitions ORDER BY id DESC LIMIT 1"), 0)) + 1;

while ($text = mysql_fetch_assoc($get))
{
	$badgeName = $text['code'];
	$badgeRights = $text['rights_sets'];

	echo '<tr><form method="post">';
	echo '<td><center>#' . $text['id']. '</center></td>';
	echo '<td><img src="' . CLIENT_BASE . '/c_images/album1584/' . $badgeName . '.gif" style="vertical-align: middle;">&nbsp;&nbsp;' . $badgeName . '</td>';
	echo '<input type="hidden" name="edit-name" value="' . $badgeName . '">';
	echo '<td><input type="text" style="width: 100%; padding: 10px; font-size: 115%;" name="rightsets" value="' . $badgeRights . '"></td>';
	echo '<td><center><input type="submit" value="Update">&nbsp;<input type="button" value="Delete" onclick="window.location = \'index.php?_cmd=badgedefs&doDel=' . $badgeName . '\';"></center></td>';
	echo '</form></tr>';
}

$tp = ceil($all / $limit);

$pagesstring = '';
$prev = $page -1;
$next = $page +1;

if ($page > 1) 
{
	$pagesstring .= "<li><a href='index.php?_cmd=badgedefs&page=1'>First Page</a></li>";
	$pagesstring .= "<li><a href='index.php?_cmd=badgedefs&page=$prev'>&laquo;</a></li>";
}
$rightindex = 3;
$leftindex = ($page > 2) ? 2 : $page - 1;
for($i = $page - $leftindex; $i < $page + $rightindex; $i++)
{
	if($i >= 1 && $i <= $tp)
	{
		$pagesstring .= "<li><a" . ($page == $i? ' class="active" ' : '' ) . " href='index.php?_cmd=badgedefs&page=$i'>$i</a></li>";
	}
}

if ($page < $tp) 
{
	$pagesstring .= "<li><a href='index.php?_cmd=badgedefs&page=$next'>&raquo;</a></li>";
	$pagesstring .= "<li><a href='index.php?_cmd=badgedefs&page=$tp'>Last Page</a></li>";
}

if($page == $tp)
{
	echo '<tr><form method="post">';
	echo '<td>#' . $total . '</td>';
	echo '<td><input type="text" style="width: 100%; padding: 10px; font-size: 115%;" name="newbadge"></td>';
	echo '<td><input type="text" style="width: 100%; padding: 10px; font-size: 115%;" name="newbadgerightsets"></td>';
	echo '<td><center><input type="submit" value="Add">';
	echo '</form></tr>';
}

echo '</thead>';
echo '</table>';
echo '<br />';
echo '<div class="center"><ul class="pagination">' . $pagesstring . '</ul></div>';
echo '</div>';

echo '<div style="float: right; width: 31%;">';
echo '<h2>Rights Sets</h2>';
echo '<br />';
echo '<table width="100%" border="1">';
echo '<thead>';
echo '<td>Set ID</td>';
echo '<td>Right ID</td>';
echo '</thead>';

$rightIDs = Array();
$rights = mysql_query("SELECT * FROM rights ORDER BY set_id,right_id ASC");
while($rightsassoc = mysql_fetch_assoc($rights))
{
	$sets = mysql_query("SELECT * FROM rights WHERE set_id = '" . $rightsassoc["set_id"] . "'");
	if(mysql_num_rows($sets) > 0 && !in_array($rightsassoc["set_id"], $rightIDs))
	{
		array_push($rightIDs, $rightsassoc["set_id"]);
	}
}

foreach($rightIDs as $id)
{
	$setsarray = Array();
	
	echo '<tr>';
	echo '<td style="width: 10%; text-align: center;">' . $id . '</td>';
	$getparentid = mysql_query("SELECT * FROM rights WHERE set_id = '$id'");
	while($assoc = mysql_fetch_assoc($getparentid))
	{
		array_push($setsarray, $assoc["right_id"]);
	}

	echo '<td style="width: 90%;">' . join("<br />", $setsarray) . '</td>';
	echo '</tr>';	
}

echo '</table>';
echo '<br />';
echo 'To manage rights sets <a href="index.php?_cmd=rightsets">click here</a>.';
echo '</div>';
echo '<div style="clear: both;"></div>';

require_once "bottom.php";

?>
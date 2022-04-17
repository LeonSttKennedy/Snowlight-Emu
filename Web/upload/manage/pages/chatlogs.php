<?php
if (!defined('IN_HK') || !IN_HK)
{
	exit;
}

if (!HK_LOGGED_IN || !$GetUsers->HasRight($GetUsers->Name2Id(USER_NAME), 'hk_moderation'))
{
	exit;
}

$searchResults = null;

if (isset($_GET['timeSearch']))
{
	$_POST['searchQuery'] = $_GET['timeSearch'];
	$_POST['searchType'] = '4';
}

if (isset($_POST['searchQuery']))
{
	$query = $_POST['searchQuery'];
	$type = $_POST['searchType'];
	
	$q = "SELECT * FROM moderation_chatlogs WHERE ";
	
	switch ($type)
	{
		default:
		case '1':
			
			$q .= "user_id = '" . $_POST['searchQuery'] . "'";
			break;
			
		case '2':
			
			$getidbyusername = intval(mysql_result(mysql_query("SELECT id FROM characters WHERE username = '" . $_POST['searchQuery'] . "' LIMIT 1"), 0));
			$q .= "user_id = '" . $getidbyusername . "'";
			break;
			
		case '3':
		
			$q .= "message LIKE '%" . $query . "%'";
			break;
			
		case '4':
		
			$q .= "room_id = '" . $query . "'";
			break;
			
		case '5':
		
			$cutMin = intval($query) - 300;
			$cutMax = intval($query) + 300;
			
			$q .= "timestamp >= " . $cutMin . " AND timestamp <= " . $cutMax;
	}
	
	$searchResults = mysql_query($q);
}

require_once "top.php";

?>			

<h1>Chatlogs</h1>

<br />

<p>
	This tool may be used to look up and review room chatlogs. Special chat such as IM and minimail is not being monitored here. Seperate tools
	may be available for them.
</p>

<br />

<p>
	<b>IMPORTANT:</b> Chatlogs are only kept for a limited period of time.<br />
	Chatlogs older than 2 weeks will be permanently deleted. If you would
	like to keep them for a longer period, please save them locally.
</p>

<?php

if (isset($searchResults))
{
	echo '<h2>Search results - You searched for "<span style="font-size: 125%;">' . $_POST['searchQuery'] . '</span>"</h2>';
	echo '<br /><p><a href="index.php?_cmd=chatlogs&doReset">Clear search</a></p><br />
	
	<table width="100%">
	<thead>
	<tr>
		<td>Date</td>
		<td>User</td>
		<td>Room</td>
		<td>Message</td>
		<td>Timestamp</td>
	</tr>
	<tbody>';
	
	while ($result = mysql_fetch_assoc($searchResults))
	{
		$getuserinfo = mysql_fetch_assoc(mysql_query("SELECT * FROM characters WHERE id = '" . $result['user_id'] . "' LIMIT 1"));
		$getroominfo = mysql_fetch_assoc(mysql_query("SELECT * FROM rooms WHERE id = '" . $result['room_id'] . "' LIMIT 1"));
		
		echo '<tr>
		<td>' . date('d/m/Y H:i:s', $result['timestamp']) . '</td>
		<td><a href="#">' . $getuserinfo['username'] . '</a> (' . $result['user_id'] . ')</td>
		<td><a href="#">' . $getroominfo['name'] . '</a> (' . $result['room_id'] . ')</td>
		<td>' . $result['message'] . '</td>
		<td>' . $result['timestamp'] . ' (<a href="index.php?_cmd=chatlogs&timeSearch=' . intval($result['timestamp']) . '">Search</a>)</td>
		</tr>';
	}
	
	echo '</tbody>
	</thead>
	</table>';
}
else
{
	echo '<h2>Search chatlogs</h2>
	
	<br />
	
	<form method="post">
	
	Search type:&nbsp;
	<select name="searchType">
	<option value="1">By user ID</option>
	<option value="2">By username</option>
	<option value="3">By message content</option>
	<option value="4">By room (by ID only!)</option>
	<option value="5">By timestamp (600 second range)</option>
	</select>
	
	<br /><br />
	
	Search query:&nbsp;
	<input type="text" name="searchQuery">
	
	<br /><br />
	
	<input type="submit" value="Search">
	
	</form>
	
	
	<h2>Recent activity</h2>
	<table width="100%">
	<thead>
	<tr>
		<td>Date</td>
		<td>User</td>
		<td>Room</td>
		<td>Message</td>
		<td>Timestamp</td>
	</tr>
	<tbody>';
	
	$getRecent = mysql_query("SELECT * FROM moderation_chatlogs ORDER BY id DESC LIMIT 30");
	
	while ($recent = mysql_fetch_assoc($getRecent))
	{
		$getuserinfo = mysql_fetch_assoc(mysql_query("SELECT * FROM characters WHERE id = '" . $recent['user_id'] . "' LIMIT 1"));
		$getroominfo = mysql_fetch_assoc(mysql_query("SELECT * FROM rooms WHERE id = '" . $recent['room_id'] . "' LIMIT 1"));
		
		echo '<tr>
		<td>' . date('d/m/Y H:i:s', $recent['timestamp']) . '</td>
		<td><a href="#">' . $getuserinfo['username'] . '</a> (' . $recent['user_id'] . ')</td>
		<td><a href="#">' . $getroominfo['name'] . '</a> (' . $recent['room_id'] . ')</td>
		<td>' . $recent['message'] . '</td>
		<td>' . $recent['timestamp'] . ' (<a href="index.php?_cmd=chatlogs&timeSearch=' . intval($recent['timestamp']) . '">Search</a>)</td>
		</tr>';
	}
	
	echo '</tbody>
	</thead>
	</table>';
}


require_once "bottom.php";

?>
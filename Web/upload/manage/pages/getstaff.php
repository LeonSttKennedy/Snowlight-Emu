<?php

if (!defined('IN_HK') || !IN_HK)
{
	exit;
}

if (!HK_LOGGED_IN)
{
	exit;
}

require_once "top.php";

?>			

<h1>Staff listing</h1>

<p>
	This is an overview of the uberHotel staff team with their contact details as defined in their account settings.
</p>

<br />

<table width="100%" border="1">
<thead>
	<td>User</td>
	<td>Rank</td>
	<td>Contact</td>
</thead>
<?php
$get = mysql_query("SELECT * FROM badges ORDER BY badge_id ASC");

while ($user = mysql_fetch_array($get))
{
	$badgedefquery = mysql_query("SELECT * FROM badge_definitions WHERE id = '$user[2]'");
	while($badgedefassoc = mysql_fetch_assoc($badgedefquery))
	{
		if($badgedefassoc['code'] == "ADM" || $badgedefassoc['code'] == "HBA" || $badgedefassoc['code'] == "NWB")
		{
			$userq = mysql_fetch_assoc(mysql_query("SELECT * FROM users WHERE id = '" . $user[1] . "' LIMIT 1"));
			$charsq = mysql_fetch_assoc(mysql_query("SELECT * FROM characters WHERE id = '" . $user[1] . "' LIMIT 1"));
			echo '<tr>';
			echo '<td>' . $charsq['username'] . '</td>';
			echo '<td>' . $GetUsers->GetRankName($user[1]) . '</td>';
			echo '<td><a href="mailto:' . $userq['account_email'] . '">' . $userq['account_email'] . '</a></td>';
			echo '</tr>';
		}
	}
}

?>
</table>

<?php

require_once "bottom.php";

?>
<?php

if (!defined('IN_HK') || !IN_HK)
{
	exit;
}

if (!HK_LOGGED_IN || !$GetUsers->HasRight($GetUsers->Name2Id(USER_NAME), 'hk_catalog'))
{
	exit;
}

$defdata = null;

if (isset($_GET['defId']) && is_numeric($_GET['defId']))
{
	$defId = intval($_GET['defId']);
	$getData = mysql_query("SELECT * FROM item_definitions WHERE id = '" . $defId . "' LIMIT 1");
	
	if (mysql_num_rows($getData) > 0)
	{
		$defdata = mysql_fetch_assoc($getData);
	}
}

if ($defdata == null)
{
	fMessage('error', 'Woops, that item definition does not exist.');
	header("Location: index.php?_cmd=itemdefs");
	exit;
}

require_once "top.php";
?>
<?php
require_once "bottom.php";
?>
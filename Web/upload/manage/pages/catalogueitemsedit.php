<?php

if (!defined('IN_HK') || !IN_HK)
{
	exit;
}

if (!HK_LOGGED_IN || !$GetUsers->HasRight($GetUsers->Name2Id(USER_NAME), 'hk_catalog'))
{
	exit;
}

$itemdata = null;

if (isset($_GET['itemId']) && is_numeric($_GET['itemId']))
{
	$itemId = intval($_GET['itemId']);
	$getData = mysql_query("SELECT * FROM catalog_items WHERE id = '" . $itemId . "' LIMIT 1");
	
	if (mysql_num_rows($getData) > 0)
	{
		$itemdata = mysql_fetch_assoc($getData);
	}
}

if ($itemdata == null)
{
	fMessage('error', 'Woops, that catalogue item does not exist.');
	header("Location: index.php?_cmd=catalogueitems");
	exit;
}

require_once "top.php";
?>
<?php
require_once "bottom.php";
?>
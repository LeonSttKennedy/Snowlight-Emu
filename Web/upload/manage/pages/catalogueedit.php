<?php

if (!defined('IN_HK') || !IN_HK)
{
	exit;
}

if (!HK_LOGGED_IN || !$GetUsers->HasRight($GetUsers->Name2Id(USER_NAME), 'hk_catalog'))
{
	exit;
}

$cataloguedata = null;

if (isset($_GET['pageId']) && is_numeric($_GET['pageId']))
{
	$pageId = intval($_GET['pageId']);
	$getData = mysql_query("SELECT * FROM catalog WHERE id = '" . $pageId . "' LIMIT 1");
	
	if (mysql_num_rows($getData) > 0)
	{
		$cataloguedata = mysql_fetch_assoc($getData);
	}
}

if ($cataloguedata == null)
{
	fMessage('error', 'Woops, that catalogue page does not exist.');
	header("Location: index.php?_cmd=catalogue");
	exit;
}

require_once "top.php";
?>
<?php
require_once "bottom.php";
?>
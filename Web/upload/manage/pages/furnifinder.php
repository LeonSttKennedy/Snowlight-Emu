<?php

if (!defined('IN_HK') || !IN_HK)
{
	exit;
}

if (!HK_LOGGED_IN || !$GetUsers->HasRight($GetUsers->Name2Id(USER_NAME), 'hk_catalog'))
{
	exit;
}

require_once "top.php";

echo '<h1>New furni finder</h1>';
echo '<p>This tool will scan Habbo UK\'s furni data file for furniture that is missing from our defs.</p><br />';
echo '<p><a href="' . CLIENT_BASE . '/furnidata.txt">' . CLIENT_BASE . '/furnidata.txt</a></p><br />';

$whatWeKnow = Array();
$getWhatWeKnow = mysql_query("SELECT name FROM item_definitions");

while ($g = mysql_fetch_assoc($getWhatWeKnow))
{
	$whatWeKnow[] = $g['name'];
}

$data = file_get_contents(CLIENT_BASE . "/furnidata.txt");
$data = str_replace("\n", "", $data);
$data = str_replace("[[", "[", $data);
$data = str_replace("]]", "]", $data);
$data = str_replace("][", "],[", $data);

$ij = 0;
$stuffWeDontKnow = Array();

foreach (explode('],[', $data) as $val)
{
	$val = str_replace('[', '', $val);
	$val = str_replace(']', '', $val);
	
	$bits = explode(',', $val);
	$name = str_replace('"', '', $bits[2]);
	
	if (in_array($name, $whatWeKnow))
	{
		continue;
	}
	
	$stuffWeDontKnow[] = '[' . $val . ']';
	$ij++;
	
	//echo $name . '<br />';
}

echo '<h2>New/missing furni (' . $ij . ')</h2><br />';

if ($ij >= 1)
{
	foreach ($stuffWeDontKnow as $stuff)
	{
		echo '<div style="padding: 5px; margin: 5px; border: 1px dotted; background-color: #F2F2F2; color: #151515;">' . $stuff . '</div><br />';
	}
}
else
{
	echo '<Br /><br /><center style="font-size: 120%;"><i><b>Good news!</b><br />I have no new furni for you today.</i></center>';
}

require_once "bottom.php";

?>								
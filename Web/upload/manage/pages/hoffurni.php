<?php

if (!defined('IN_HK') || !IN_HK)
{
	exit;
}

if (!HK_LOGGED_IN || !$GetUsers->HasRight($GetUsers->Name2Id(USER_NAME), 'hotel_admin'))
{
	exit;
}

function getExtension($str)
{
	$i = strrpos($str, '.');
	
	if (!$i)
	{
		return '';
	}
	
	$l = strlen($str) - $i;
	$ext = substr($str, $i + 1, $l);
	
	return $ext;
}

if (isset($_POST['name']))
{
	$image = $_FILES['img']['name'];
	
	if ($image && isset($_POST['name']) && isset($_POST['clickurl']))
	{
		$filename = strtolower($_POST['name']);
		$clickurl = $_POST['clickurl'];
		
		if (strlen($filename) >= 1 && strlen($clickurl) >= 1)
		{
			$ext = getExtension(strtolower($_FILES['img']['name']));
			
			$fileLoc = CWD . '/ads/' . $filename . '.' . $ext;
			$wwwLoc = SITE_DOMAIN . '/ads/' . $filename . '.' . $ext;
			
			if ($ext == "gif")
			{
				if (copy($_FILES['img']['tmp_name'], $fileLoc))
				{
					mysql_query("INSERT INTO `interstitials` (`id`, `url`, `image`, `image_orig`, `views`, `enabled`) VALUES (NULL,'" . $clickurl . "','" . $wwwLoc . "','" . $filename . '.' . $ext . "',0,1)");
					fMessage('ok', 'Okay, interstitial ad uploaded.');
				}
				else
				{
					fMessage('error', 'Could not process file: ' . $fileLoc);
				}
			}
			else
			{
				fMessage('error', 'Invalid file type: ' . $ext);
			}
		}
		else
		{
			fMessage('error', 'Please enter a file name and URL.');
		}
	}
	else
	{
		fMessage('error', 'File upload error (unknown).');
	}
}

if (isset($_GET['delId']))
{
	mysql_query("DELETE FROM interstitials WHERE image_orig = '" . $_GET['delId'] . "'");
	
	if (@unlink(CWD . '/ads/' . $_GET['delId']))
	{
		fMessage('ok', 'Deleted interstitial ad.');
	}
	
	header("Location: index.php?_cmd=roomads");
	exit;
}

if (isset($_GET['switchId']))
{
	$get = mysql_query("SELECT enabled FROM interstitials WHERE image_orig = '" . $_GET['switchId'] . "' LIMIT 1");
	
	if (mysql_num_rows($get) >= 1)
	{
		$enabled = mysql_result($get, 0);
		
		$set = "0";
		
		if ($enabled == "0")
		{
			$set = "1";
		}

		mysql_query("UPDATE interstitials SET enabled = '" . $set . "' WHERE image_orig = '" . $_GET['switchId'] . "' LIMIT 1");
	}
	
	header("Location: index.php?_cmd=roomads");
	exit;	
}

require_once "top.php";

define('DS', DIRECTORY_SEPARATOR);
define('CWDD', str_replace('RainbowCMS' . DS . 'manage' . DS . 'pages'. DS, '', dirname(__FILE__) . DS));
?>			

<h1>Interstitials</h1>

<p>
	Interstitials are advertisements shown when users navigate between rooms, while the new room loads.
</p>

<h2>Ad gallery</h2>

<br />

<?php

$handle = null;
$i = 0;
if ($handle = opendir(CWDD . '/cdn.classichabbo.com/r38/gordon/RELEASE63-35255-34886-201108111108_ce2d130905ba279edbfb4208cd5035c0/hof_furni'))
{
	while (false !== ($file = readdir($handle)))
	{
		$file = strtolower($file);
	
		if ($file == '.' || $file == '..' || getExtension($file) != "swf" || strpos($file, 'poster') !== false)
		{
			continue;
		}

		$file = str_replace('.swf', '', $file);
		
		$hasDbEntry = false;
		$dbData = null;
		$dbGet = mysql_query("SELECT * FROM item_definitions WHERE name LIKE '%" . $file . "%' LIMIT 1");
		
		if (mysql_num_rows($dbGet) >= 1)
		{
			$hasDbEntry = true;
			$dbData = mysql_fetch_assoc($dbGet);
		}
		
		if ($hasDbEntry == false && $dbData == null)
		{
			echo $file . "<br />";
			$i++;
		}
	}
	
	closedir($handle);
	echo "<br />
	Founded " . $i . " item(s)";
}
	
require_once "bottom.php";

?>
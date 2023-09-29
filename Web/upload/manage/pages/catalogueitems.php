<?php

if (!defined('IN_HK') || !IN_HK)
{
	exit;
}

if (!HK_LOGGED_IN || !$GetUsers->HasRight($GetUsers->Name2Id(USER_NAME), 'hk_catalog'))
{
	exit;
}

/*function PublicItemName($itemname)
{
	$return = "NAME NOT FOUND";
	
	$data = file_get_contents(CLIENT_BASE . "/productdata.txt");
	$data = str_replace("\n", "", $data);
	$data = str_replace("[[", "[", $data);
	$data = str_replace("]]", "]", $data);
	$data = str_replace("][", "],[", $data);
	
	foreach (explode('],[', $data) as $val)
	{
		$val = str_replace('[', '', $val);
		$val = str_replace(']', '', $val);
	
		$bits = explode(',', $val);
		$name = str_replace('"', '', $bits[0]);
		
		if ($itemname == $name)
		{
			$return = str_replace('"', '', $bits[1]);
		}
	}
	
	return $return;
}*/

function ClubRestrictionString($restrictionid)
{
	switch($restrictionid)
	{
		default:
		case 0:
			$restriction = 'For Everyone';
			break;
				
		case 1:
			$restriction = 'For HC Members';
			break;
				
		case 2:
			$restriction = 'For VIP Members';
			break;
	}
	
	return $restriction;
}

function SeasonalCurrency($currency)
{
	switch($currency)
	{
		case "pixels":
			$str_retun = 'Pixels';
			break;
				
		case "snowflakes":
			$str_retun = 'Snowflakes';
			break;
				
		case "hearts":
			$str_retun = 'Hearts';
			break;
							
		case "giftpoints":
			$str_retun = 'Gift Points';
			break;
										
		case "shells":
			$str_retun = 'Shells';
			break;
	}
	
	return $str_retun;
}

$limit = 15;
$page = ((isset($_GET['page'])) ? $_GET['page'] : 1);

if((isset($_POST['page_id']) || isset($_POST['enabled']) || isset($_POST['name']) || isset($_POST['cost_credits_max']) || isset($_POST['cost_credits_min']) || isset($_POST['cost_pixels_max']) || isset($_POST['cost_pixels_min'])) && isset($_GET['page']))
{
	unset($_GET['page']);
	$page = 1;
}

$start = $page - 1;
$start = $start * $limit;

if(isset($_POST['clearSearch']))
{
	foreach($searchparams as $searchkey => $searchvalue) 
	{
		$searchparams[$searchkey] = "";
	}
}

$maxcredits = mysql_result(mysql_query("SELECT cost_credits FROM catalog_items ORDER BY cost_credits DESC LIMIT 1") , 0);
$maxpixels = mysql_result(mysql_query("SELECT cost_pixels FROM catalog_items ORDER BY cost_pixels DESC LIMIT 1") , 0);

$searchparams = array(
	'page_id' => (isset($_POST['page_id']) && is_numeric($_POST['page_id'])) ? $_POST['page_id'] : "",
	'enabled' => (isset($_POST['enabled']) && is_numeric($_POST['enabled'])) ? $_POST['enabled'] : "",
	'name' => (isset($_POST['name']) && strlen($_POST['name']) > 0) ? $_POST['name'] : "",
	'club_restriction' => (isset($_POST['club_restriction']) && is_numeric($_POST['club_restriction'])) ? $_POST['club_restriction'] : "",
	'cost_credits_min' => (isset($_POST['cost_credits_min']) && is_numeric($_POST['cost_credits_min']) && $_POST['cost_credits_min'] > 0 && $_POST['cost_credits_min'] <= $maxcredits) ? $_POST['cost_credits_min'] : "",
	'cost_credits_max' => (isset($_POST['cost_credits_max']) && is_numeric($_POST['cost_credits_max']) && $_POST['cost_credits_max'] > 0 && $_POST['cost_credits_max'] <= $maxcredits) ? $_POST['cost_credits_max'] : "",
	'cost_pixels_min' => (isset($_POST['cost_credits_min']) && is_numeric($_POST['cost_pixels_min']) && $_POST['cost_pixels_min'] > 0 && $_POST['cost_pixels_min'] <= $maxpixels) ? $_POST['cost_pixels_min'] : "",
	'cost_pixels_max' => (isset($_POST['cost_credits_max']) && is_numeric($_POST['cost_pixels_max']) && $_POST['cost_pixels_max'] > 0 && $_POST['cost_pixels_max'] <= $maxpixels) ? $_POST['cost_pixels_max'] : ""
);

if(isset($_GET['query']))
{
	$query = explode(",", $_GET['query']);
	foreach($searchparams as $searchkey => $searchvalue) 
	{
		foreach($query as $string)
		{
			if(!isset($_POST[substr($string, strpos($string, "-") + 1)]) && $searchkey == substr($string, strpos($string, "-") + 1))
			{
				$searchparams[substr($string, strpos($string, "-") + 1)] = substr($string, 0, strlen($string) - strlen(substr($string, strpos($string, "-"))));
			}
		}
	}
}

$catalogueitemsquerystring .= "SELECT * FROM catalog_items ";
if(is_numeric($searchparams['page_id']) || strlen($searchparams['name']) > 0 || is_numeric($searchparams['club_restriction']) || is_numeric($searchparams['cost_credits_max']) || is_numeric($searchparams['cost_credits_min']) || is_numeric($searchparams['cost_pixels_max']) || is_numeric($searchparams['cost_pixels_min']) ||  is_numeric($searchparams['enabled']))
{
	foreach($searchparams as $searchkey => $searchvalue) 
	{
		switch($searchkey)
		{
			case 'page_id':
			case 'enabled':
			case 'club_restriction':
				if(is_numeric($searchvalue))
				{
					$searchstring[] = "$searchkey = '$searchvalue'";
				}
			continue;
			
			case 'cost_credits_min':
				if(is_numeric($searchparams["cost_credits_min"]) && $searchparams["cost_credits_min"] > 0)
				{
					$searchstring[] = "cost_credits >= '" . $searchparams['cost_credits_min'] . "'";
				}
				continue;
				
			case 'cost_credits_max':
				if(is_numeric($searchparams["cost_credits_max"]) && $searchparams["cost_credits_max"] > 0)
				{
					$searchstring[] = "cost_credits <= '" . $searchparams['cost_credits_max'] . "'";
				}
				continue;
				
			case 'cost_pixels_min':
				if(is_numeric($searchparams["cost_pixels_min"]) && $searchparams["cost_pixels_min"] > 0)
				{
					$searchstring[] = "cost_pixels >= '" . $searchparams['cost_pixels_min'] . "'";
				}
				continue;
			
			case 'cost_pixels_max':
				if(is_numeric($searchparams["cost_pixels_max"]) && $searchparams["cost_pixels_max"] > 0)
				{
					$searchstring[] = "cost_pixels <= '" . $searchparams['cost_pixels_max'] . "'";
				}
				continue;
			
		}
		
		if(strlen($searchvalue) > 0 && !is_numeric($searchvalue))
		{
			$searchstring[] = "$searchkey LIKE '%$searchvalue%'";
		}
	}
	
	$orderby = (is_numeric($searchparams['page_id'])) ? " ORDER BY catalog_item_order ASC" : "";
	
	$whreCase = join(" AND ", $searchstring);
	if(strlen($whreCase) > 0)
	{
		$allwhreCase = " WHERE " . $whreCase . $orderby;
		$catalogueitemsquerystring .= "WHERE " . $whreCase . $orderby . " ";
	}
}

$catalogueitemsquerystring .= "LIMIT $start,$limit";
$catalogueitemsquery = mysql_query($catalogueitemsquerystring);
$all = intval(mysql_num_rows(mysql_query("SELECT * FROM catalog_items" . $allwhreCase)));

if (isset($_POST['update-order']))
{
	foreach ($_POST as $key => $value)
	{
		if ($key == 'update-order')
		{
			continue;
		}
	
		if (substr($key, 0, 4) != 'ord-')
		{
			die("Invalid: " . $key);
			continue;
		}
		
		$id = intval(substr($key, 4));

		mysql_query("UPDATE catalog_items SET order_num = '" . intval($value) . "' WHERE id = '" . $id .  "' LIMIT 1");
	}
	
	fMessage('ok', 'Updated page order.');
}

require_once "top.php";
?>

<h1>Catalogue Items</h1>

<br />

<p>
	This tool you can see all items that are inserted in catalog pages.
</p>

<h2>Search a catalogue item</h2><br />
<form method="post">
Item name:&nbsp;&nbsp;<input type="text" name="name" value="<?php echo ((strlen($searchparams['name']) > 0) ?  $searchparams['name'] : ''); ?>"><br /><br />
Parent page:&nbsp;&nbsp;<select name="page_id">
<option value="" disabled selected hidden></option>
<?php
$pageIDs = Array();
$getcatalogpages = mysql_query("SELECT * FROM catalog ORDER BY title ASC");

while($pageid = mysql_fetch_array($getcatalogpages))
{
	$getcatalogitems = mysql_query("SELECT * FROM catalog_items WHERE page_id = '" . $pageid['id'] . "'");
	if(mysql_num_rows($getcatalogitems) > 0 && !in_array($pageid["id"], $pageIDs))
	{
		array_push($pageIDs, $pageid["id"]);
	}
}

foreach($pageIDs as $id)
{
	$getparentid = mysql_fetch_assoc(mysql_query("SELECT * FROM catalog WHERE id = '" . $id . "' LIMIT 1"));
	echo '<option value="' . intval($getparentid['id']) . '" ' . ($getparentid['id'] == $searchparams['page_id'] ? 'selected' : '') . '>' . $getparentid['title'] . '</option>';
}
?>
</select><br /><br />
Club Restriction:&nbsp;&nbsp;<select name="club_restriction">
<option value="" disabled selected hidden></option>
<?php
$club_restrictionArray = array();
$club_restrictionQuery = mysql_query("SELECT * FROM catalog_items ORDER BY club_restriction ASC");
while($club_restrictionassoc = mysql_fetch_assoc($club_restrictionQuery))
{
	if(!in_array($club_restrictionassoc["club_restriction"], $club_restrictionArray))
	{
		array_push($club_restrictionArray, $club_restrictionassoc["club_restriction"]);
	}
}

foreach($club_restrictionArray as $restriction)
{
	echo '<option value="' . $restriction . '" ' . ($restriction == $searchparams["club_restriction"] ? "selected" : "") . '>' . ClubRestrictionString($restriction) . '</option>';
}
?>
</select><br /><br />
<p>Minimum value:&nbsp;&nbsp;
Credits:&nbsp;&nbsp;<input type="number" name="cost_credits_min" max="<?php echo $maxcredits; ?>" value="<?php echo ((is_numeric($searchparams['cost_credits_min']) && $searchparams['cost_credits_min'] > 0) ?  $searchparams['cost_credits_min'] : '0'); ?>">&nbsp;&nbsp;Pixels:&nbsp;&nbsp;<input type="number" name="cost_pixels_min" max="<?php echo $maxpixels; ?>" value="<?php echo ((is_numeric($searchparams['cost_pixels_min']) && $searchparams['cost_pixels_min'] > 0) ?  $searchparams['cost_pixels_min'] : '0'); ?>"></p><br />
<p>Maximum value:&nbsp;&nbsp;
Credits:&nbsp;&nbsp;<input type="number" name="cost_credits_max" max="<?php echo $maxcredits; ?>" value="<?php echo ((is_numeric($searchparams['cost_credits_max']) && $searchparams['cost_credits_max'] > 0) ?  $searchparams['cost_credits_max'] : '0'); ?>">&nbsp;&nbsp;Pixels:&nbsp;&nbsp;<input type="number" name="cost_pixels_max" max="<?php echo $maxpixels; ?>" value="<?php echo ((is_numeric($searchparams['cost_pixels_max']) && $searchparams['cost_pixels_max'] > 0) ?  $searchparams['cost_pixels_max'] : '0'); ?>"></p><br />
Enabled:&nbsp;&nbsp;<input type="radio" name="enabled" <?php echo ((is_numeric($searchparams['enabled']) && $searchparams['enabled'] == 1) ? 'checked' : ''); ?> value="1">&nbsp;True&nbsp;<input type="radio" name="enabled" <?php echo ((is_numeric($searchparams['enabled']) && $searchparams['enabled'] == 0) ? 'checked' : ''); ?> value="0">&nbsp;False<br /><br />
<input type="submit" value="Search">&nbsp;&nbsp;<input type="button" value="Clear" onclick="window.location = 'index.php?_cmd=catalogueitems&clearSearch';">
</form>
<br />
<hr />
<h2>Catalogue Items List</h2>
<br />
<table border="1" width="100%">
	<thead>
		<td>ID</td>		
		<td>Page Name</td>
		<td>Order</td>
		<td>Item Name</td>
		<td>Cost</td>
		<td>Sazonal Currency</td>
		<td>Enabled</td>
		<td>Amount</td>
		<td>Sale Availability</td>
		<td>Controls</td>
	</thead>
	<?php
	echo ((is_numeric($searchparams['page_id']) && $searchparams['name'] == "" && $searchparams['cost_credits_min'] < 1 && $searchparams['cost_credits_max'] < 1  && $searchparams['cost_pixels_max'] < 1 && $searchparams['cost_pixels_min'] < 1 && $searchparams['enabled'] == "") ? '<form method="post">' : '');
	while($item = mysql_fetch_assoc($catalogueitemsquery))
	{
		$parentpageid = mysql_result(mysql_query("SELECT page_id FROM catalog_items WHERE id = '" . $item["id"] . "' LIMIT 1"), 0);
		$parentpagecaption = mysql_result(mysql_query("SELECT title FROM catalog WHERE id = '" . $parentpageid . "' LIMIT 1"), 0);
		
		echo '<tr>';
		echo '<td>#' . $item["id"] . '</td>';
		echo '<td>' . (strlen($parentpagecaption) > 0 ?  $parentpagecaption : '-') . '</td>';
		echo '<td>' . ((is_numeric($searchparams['page_id']) && $searchparams['name'] == "" && $searchparams['cost_credits_min'] < 1 && $searchparams['cost_credits_max'] < 1  && $searchparams['cost_pixels_max'] < 1 && $searchparams['cost_pixels_min'] < 1 && $searchparams['enabled'] == "") ? '<input style="text-align: center; font-weight: bold; margin: 2px;" type="text" size="3" value="' . $item['catalog_item_order'] . '" name="ord-' . $item['id'] . '">' : $item["catalog_item_order"]) . '</td>';
		echo '<td>' . $item["name"] . '</td>';
		echo '<td><center>' . $item["cost_credits"] . '&nbsp;CR&nbsp;&&nbsp;' . $item["cost_activitypoints"] . '&nbsp;AP</center></td>';
		echo '<td><center>'. SeasonalCurrency($item["seasonal_currency"]) . '</center></td>';
		echo '<td>' . ($item["enabled"] == "1" ? 'Yes' : 'No') . '</td>';
		echo '<td>' . $item["amount"] . '</td>';
		echo '<td>' . ClubRestrictionString($item['club_restriction']) . '</td>';
		echo '<td><input type="button" value="Edit" onclick="window.location = \'index.php?_cmd=catalogueitemsedit&itemId=' . $item["id"] . '\';">&nbsp;<input type="button" value="Delete" onclick="if(confirm(\'Do you really want to delete this item ' . $item["name"] . ' ( #' . $item["id"] . ' )\ ?\')){window.location = \'index.php?_cmd=catalogueitems&doDel=' . $item["id"] . '\';}"></td>';
		echo '</tr>';
	}
	
	?>
</table>
<?php
if(strlen($allwhreCase) > 0)
{
	foreach($searchparams as $searchkey => $searchvalue) 
	{
		if(strlen($searchvalue) > 0)
		{
			$ss[] = "$searchvalue-$searchkey";
		}
	}
	
	$squery = "&query=" . join(",", $ss);
}

$tp = ceil($all / $limit);

$pagesstring = '';
$prev = $page -1;
$next = $page +1;

if ($page > 1) 
{
	$pagesstring .= "<li><a href='index.php?_cmd=catalogueitems$squery&page=1'>First Page</a></li>";
	$pagesstring .= "<li><a href='index.php?_cmd=catalogueitems$squery&page=$prev'>&laquo;</a></li>";
}
$rightindex = 3;
$leftindex = ($page > 2) ? 2 : $page - 1;
for($i = $page - $leftindex; $i < $page + $rightindex; $i++)
{
	if($i >= 1 && $i <= $tp)
	{
		$pagesstring .= "<li><a" . ($page == $i? ' class="active" ' : '' ) . " href='index.php?_cmd=catalogueitems$squery&page=$i'>$i</a></li>";
	}
}

if ($page < $tp) 
{
	$pagesstring .= "<li><a href='index.php?_cmd=catalogueitems$squery&page=$next'>&raquo;</a></li>";
	$pagesstring .= "<li><a href='index.php?_cmd=catalogueitems$squery&page=$tp'>Last Page</a></li>";
}

echo ((is_numeric($searchparams['page_id']) && $searchparams['name'] == "" && $searchparams['cost_credits_min'] < 1 && $searchparams['cost_credits_max'] < 1  && $searchparams['cost_pixels_max'] < 1 && $searchparams['cost_pixels_min'] < 1 && $searchparams['enabled'] == "") ? '<br /><input type="submit" name="update-order" value="Save items order"> or <input type="button" value="Cancel/Reset" onclick="location.reload(true);"></form>' : '');
echo '<div class="center"><ul class="pagination">' . $pagesstring . '</ul></div>';

require_once "bottom.php";
?>
<?php

if (!defined('IN_HK') || !IN_HK)
{
	exit;
}

if (!HK_LOGGED_IN || !$GetUsers->HasRight($GetUsers->Name2Id(USER_NAME), 'hk_catalog'))
{
	exit;
}

$limit = 15;
$page = ((isset($_GET['page'])) ? $_GET['page'] : 1);

if((isset($_POST['name'])) && isset($_GET['page']))
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

$searchparams = array(
	'name' => (isset($_POST['name']) && strlen($_POST['name']) > 0) ? $_POST['name'] : "",
	'behavior' => (isset($_POST['behavior']) && strlen($_POST['behavior']) > 0) ? $_POST['behavior'] : "",
	'cataentry' => (isset($_POST['cataentry']) && is_numeric($_POST['cataentry'])) ? $_POST['cataentry'] : ""
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

$itemdefsquerystring .= "SELECT * FROM item_definitions ";
if(strlen($searchparams['name']) > 0 || strlen($searchparams['behavior']) > 0 || is_numeric($searchparams["cataentry"]))
{
	foreach($searchparams as $searchkey => $searchvalue) 
	{
		switch($searchkey)
		{
			case 'page_id':
			case 'enabled':
				if(is_numeric($searchvalue))
				{
					$searchstring[] = "$searchkey = '$searchvalue'";
				}
			continue;
			
			case 'cataentry':
				if(is_numeric($searchparams["cataentry"]))
				{
					$defidsarray = array();
					$totaldefitems = mysql_query("SELECT * FROM item_definitions");
					while($d = mysql_fetch_assoc($totaldefitems))
					{
						$totalcataitems = mysql_query("SELECT * FROM catalog_items WHERE base_id = '" . $d['id'] . "'");
						if($searchparams["cataentry"] == 1 && mysql_num_rows($totalcataitems) > 0)
						{
							array_push($defidsarray, $d["id"]);
						}
						else if($searchparams["cataentry"] == 0 && mysql_num_rows($totalcataitems) <= 0)
						{
							array_push($defidsarray, $d["id"]);
						}
					}
					
					if(count($defidsarray) > 0)
					{
						$searchstring[] = "id IN (" . implode(', ', $defidsarray) . ")";
					}
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
		$itemdefsquerystring .= "WHERE " . $whreCase . $orderby . " ";
	}
}

$itemdefsquerystring .= "LIMIT $start,$limit";

$defsquery = mysql_query($itemdefsquerystring);
$all = intval(mysql_num_rows(mysql_query("SELECT * FROM item_definitions" . $allwhreCase)));

require_once "top.php";
?>

<h1>Item Definitions</h1>

<br />

<p>
	This tool you can see all item definitions from furniture in hotel.
</p>

<div style="float: left; width: 67%;">
<h2>Search a item definition</h2><br />
<form method="post">
Item name:&nbsp;&nbsp;<input type="text" name="name" value="<?php echo ((strlen($searchparams['name']) > 0) ?  $searchparams['name'] : ''); ?>"><br /><br />
<p>Interaction Type:&nbsp;
<select name="behavior">
<option value="" disabled selected hidden></option>
<?php 
$behaviorArray = array();
$behaviorsquery = mysql_query("SELECT * FROM item_definitions ORDER BY behavior ASC");
while($behaviorassoc = mysql_fetch_assoc($behaviorsquery))
{
	if(!in_array($behaviorassoc["behavior"], $behaviorArray))
	{
		array_push($behaviorArray, $behaviorassoc["behavior"]);
	}
}

foreach($behaviorArray as $b)
{
	echo '<option value="' . $b . '" ' . ($b == $searchparams["behavior"] ? "selected" : "") . '>' . ucfirst($b) . '</option>';
}
?>
</select></p><br />
<p>Has Catalogue Entry:&nbsp;&nbsp;<input type="radio" name="cataentry" <?php echo ((is_numeric($searchparams['cataentry']) && $searchparams['cataentry'] == 1) ? 'checked' : ''); ?> value="1">&nbsp;Yes&nbsp;<input type="radio" name="cataentry" <?php echo ((is_numeric($searchparams['cataentry']) && $searchparams['cataentry'] == 0) ? 'checked' : ''); ?> value="0">&nbsp;No</p><br />
<input type="submit" value="Search">&nbsp;&nbsp;<input type="button" value="Clear" onclick="window.location = 'index.php?_cmd=itemdefs&clearSearch';">
</form>
</div>
<div style="float: right; width: 31%;">
<h2>Notes</h2>
<br />
<div style="border-style: solid; border-width: 0 0 0 2px; border-color:darkred; padding: 0 0 0 5px; margin: 0 0 8px 0;">
	<p>
		<b style="color: darkred; font-size: 25px; padding: 0 0 0 0; margin: 0 8px 0 0;">&RightArrow;</b>The search parameter "<u>Has Catalogue Entry</u>" may take a while to return records.
	</p>
</div>
</div>
<div style="clear: both;"></div>
<br />
<hr />
<h2>Items Definition List</h2>
<br />
<table border="1" width="100%">
	<thead>
		<td>ID</td>		
		<td>Internal Name</td>
		<td>Type</td>
		<td>Interaction Type</td>
		<td>Has Catalogue Entry</td>
		<td>Controls</td>
	</thead>
	<?php
	while($defs = mysql_fetch_assoc($defsquery))
	{
		$hascataentry = mysql_query("SELECT * FROM catalog_items WHERE base_id = '" . $defs['id'] . "'");
		
		echo '<tr>';
		echo '<td>#' . $defs["id"] . '</td>';
		echo '<td>' . $defs["name"] . '</td>';
		echo '<td>' . $defs["type"] . '</td>';
		echo '<td>' . $defs["behavior"] . '</td>';
		echo '<td>' . ((mysql_num_rows($hascataentry) > 0) ? "<b style='color: darkgreen;'>YES</b>" : "<b style='color: darkred;'>NO</b>") . '</td>';
		echo '<td><input type="button" value="Edit" onclick="window.location = \'index.php?_cmd=itemdefsedit&defId=' . $defs["id"] . '\';">&nbsp;<input type="button" value="Delete" onclick="if(confirm(\'Do you really want to delete this item ' . $defs["name"] . ' ( #' . $defs["id"] . ' )\ ?\')){window.location = \'index.php?_cmd=itemdefs&doDel=' . $defs["id"] . '\';}"></td>';
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
	$pagesstring .= "<li><a href='index.php?_cmd=itemdefs$squery&page=1'>First Page</a></li>";
	$pagesstring .= "<li><a href='index.php?_cmd=itemdefs$squery&page=$prev'>&laquo;</a></li>";
}
$rightindex = 3;
$leftindex = ($page > 2) ? 2 : $page - 1;
for($i = $page - $leftindex; $i < $page + $rightindex; $i++)
{
	if($i >= 1 && $i <= $tp)
	{
		$pagesstring .= "<li><a" . ($page == $i? ' class="active" ' : '' ) . " href='index.php?_cmd=itemdefs$squery&page=$i'>$i</a></li>";
	}
}

if ($page < $tp) 
{
	$pagesstring .= "<li><a href='index.php?_cmd=itemdefs$squery&page=$next'>&raquo;</a></li>";
	$pagesstring .= "<li><a href='index.php?_cmd=itemdefs$squery&page=$tp'>Last Page</a></li>";
}

echo '<div class="center"><ul class="pagination">' . $pagesstring . '</ul></div>';

require_once "bottom.php";
?>
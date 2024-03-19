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

if((isset($_POST['parent_id']) || isset($_POST['enabled']) || isset($_POST['title']) || isset($_POST['coming_soon'])) && isset($_GET['page']))
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
	'parent_id' => isset($_POST['parent_id']) && is_numeric($_POST['parent_id']) ? $_POST['parent_id'] : "",
	'enabled' => isset($_POST['enabled']) ? $_POST['enabled'] : "",
	'title' => isset($_POST['title']) ? $_POST['title'] : "",
	'coming_soon' => isset($_POST['coming_soon']) ? $_POST['coming_soon'] : ""
);

if(isset($_GET['query']))
{
	$query = explode(",", $_GET['query']);
	foreach($searchparams as $searchkey => $searchvalue) 
	{
		foreach($query as $string)
		{
			if(!isset($_POST[substr($string, strpos($string, "*") + 1)]) && $searchkey == substr($string, strpos($string, "*") + 1))
			{
				$searchparams[substr($string, strpos($string, "*") + 1)] = substr($string, 0, strlen($string) - strlen(substr($string, strpos($string, "*"))));
			}
		}
	}
}

$cataloguepagesquerystring .= "SELECT * FROM catalog ";
if(is_numeric($searchparams['parent_id']) || strlen($searchparams['title']) > 0 || is_numeric($searchparams['coming_soon']) ||  is_numeric($searchparams['enabled']))
{
	foreach($searchparams as $searchkey => $searchvalue) 
	{
		if(is_numeric($searchvalue))
		{
			$searchstring[] = "$searchkey = '$searchvalue'";
		}
		
		if(strlen($searchparams['title']) > 0)
		{
			$searchstring[] = "$searchkey LIKE '%$searchvalue%'";
		}
	}
	
	$orderby = (is_numeric($searchparams['parent_id'])) ? " ORDER BY order_num ASC" : "";
	
	$whreCase = join(" AND ", $searchstring);
	if(strlen($whreCase) > 0)
	{
		$allwhreCase = " WHERE " . $whreCase . $orderby;
		$cataloguepagesquerystring .= "WHERE " . $whreCase . $orderby . " ";
	}
}

$cataloguepagesquerystring .= "LIMIT $start,$limit";
$catalogquery = mysql_query($cataloguepagesquerystring);
$all = intval(mysql_num_rows(mysql_query("SELECT * FROM catalog" . $allwhreCase)));

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

		mysql_query("UPDATE catalog SET order_num = '" . intval($value) . "' WHERE id = '" . $id .  "' LIMIT 1");
	}
	
	fMessage('ok', 'Updated page order.');
}

require_once "top.php";
?>

<h1>Catalogue Pages</h1>

<br />

<p>
	This tool you can see all pages from catalogue.
</p>

<h2>Search a catalogue page</h2><br />
<form method="post">
Page title:&nbsp;&nbsp;<input type="text" name="title" value="<?php echo ((strlen($searchparams['title']) > 0) ?  $searchparams['title'] : ''); ?>"><br /><br />
Parent page:&nbsp;&nbsp;<select name="parent_id">
<option value="" disabled selected hidden></option>
<option value="-1" <?php echo ($searchparams['parent_id'] == -1 ? 'selected' : ''); ?>>Root Pages</option>
<?php

$getparentid = mysql_query("SELECT * FROM catalog WHERE parent_id = '-1' ORDER BY order_num ASC");

while ($option = mysql_fetch_assoc($getparentid))
{
	echo '<option value="' . intval($option['id']) . '" ' . ($option['id'] == $searchparams['parent_id'] ? 'selected' : '') . '>' . $option['title'] . '</option>';
}
?></select><br /><br />
Coming soon:&nbsp;&nbsp;<input type="radio" name="coming_soon" <?php echo ((is_numeric($searchparams['coming_soon']) && $searchparams['coming_soon'] == 1) ? 'checked' : ''); ?> value="1">&nbsp;True&nbsp;<input type="radio" name="coming_soon" <?php echo ((is_numeric($searchparams['coming_soon']) && $searchparams['coming_soon'] == 0) ? 'checked' : ''); ?> value="0">&nbsp;False<br /><br />
Enabled:&nbsp;&nbsp;<input type="radio" name="enabled" <?php echo ((is_numeric($searchparams['enabled']) && $searchparams['enabled'] == 1) ? 'checked' : ''); ?> value="1">&nbsp;True&nbsp;<input type="radio" name="enabled" <?php echo ((is_numeric($searchparams['enabled']) && $searchparams['enabled'] == 0) ? 'checked' : ''); ?> value="0">&nbsp;False<br /><br />
<input type="submit" value="Search">&nbsp;&nbsp;<input type="button" value="Clear" onclick="window.location = 'index.php?_cmd=catalogue&clearSearch';">
</form>
<br />
<hr />
<h2>Catalogue Pages List</h2>
<br />
<table border="1" width="100%">
	<thead>
		<td>ID</td>
		<td>Parent Page</td>
		<td>Order Num</td>
		<td>Enabled</td>
		<td>Page Title</td>
		<td>Icon</td>
		<td>Color</td>
		<td>Required Right</td>
		<td>Visible</td>
		<td>Dummy Page</td>
		<td>Coming Soon</td>
		<td>Template</td>
		<td>Page Strings 1</td>
		<td>Page Strings 2</td>
		<td>Controls</td>
	</thead>
	<?php
	echo ((is_numeric($searchparams['parent_id']) && $searchparams['title'] == "" && $searchparams['coming_soon'] == "" && $searchparams['enabled'] == "") ? '<form method="post">' : '');
	while($catalogue = mysql_fetch_assoc($catalogquery))
	{
		$parentpagecaption = mysql_result(mysql_query("SELECT title FROM catalog WHERE id = '" . $catalogue["parent_id"] . "' LIMIT 1"), 0);
		
		echo '<tr>';
		echo '<td>#' . $catalogue["id"] . '</td>';
		echo '<td>' . (strlen($parentpagecaption) > 0 ?  $parentpagecaption : '-') . '</td>';
		echo '<td>' . ((is_numeric($searchparams['parent_id']) && $searchparams['title'] == "" && $searchparams['coming_soon'] == "" && $searchparams['enabled'] == "") ? '<input style="text-align: center; font-weight: bold; margin: 2px;" type="text" size="3" value="' . $catalogue['order_num'] . '" name="ord-' . $catalogue['id'] . '">' : $catalogue["order_num"]) . '</td>';
		echo '<td>' . ($catalogue["enabled"] == "1" ? 'Yes' : 'No') . '</td>';
		echo '<td>' . $catalogue["title"] . '</td>';
		echo '<td>&nbsp;<img src="' . CLIENT_BASE . '/c_images/catalogue/icon_' . $catalogue["icon"]  . '.png" /></td>';
		echo '<td>' . $catalogue["color"] . '</td>';
		echo '<td>' . $catalogue["required_right"] . '</td>';
		echo '<td>' . ($catalogue["visible"] == "1" ? 'Yes' : 'No') . '</td>';
		echo '<td>' . ($catalogue["dummy_page"] == "1" ? 'Yes' : 'No') . '</td>';
		echo '<td>' . ($catalogue["coming_soon"] == "1" ? 'Yes' : 'No') . '</td>';
		echo '<td>' . $catalogue["template"] . '</td>';
		echo '<td>' . substr($catalogue["page_strings_1"], 0, 10) . (strlen($catalogue["page_strings_1"]) > 0 ?  '...' : '-') . '</td>';
		echo '<td>' . substr($catalogue["page_strings_2"], 0, 10) . (strlen($catalogue["page_strings_2"]) > 0 ?  '...' : '-') . '</td>';
		echo '<td><input type="button" value="Edit" onclick="window.location = \'index.php?_cmd=catalogueedit&pageId=' . $catalogue["id"] . '\';">&nbsp;<input type="button" value="Delete" onclick="if(confirm(\'Do you really want to delete the page ' . $catalogue["title"] . ' ( #' . $catalogue["id"] . ' )\ ?\')){window.location = \'index.php?_cmd=catalogue&doDel=' . $catalogue["id"] . '\';}"></td>';
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
			$ss[] = "$searchvalue*$searchkey";
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
	$pagesstring .= "<li><a href='index.php?_cmd=catalogue$squery&page=1'>First Page</a></li>";
	$pagesstring .= "<li><a href='index.php?_cmd=catalogue$squery&page=$prev'>&laquo;</a></li>";
}
$rightindex = 3;
$leftindex = ($page > 2) ? 2 : $page - 1;
for($i = $page - $leftindex; $i < $page + $rightindex; $i++)
{
	if($i >= 1 && $i <= $tp)
	{
		$pagesstring .= "<li><a" . ($page == $i? ' class="active" ' : '' ) . " href='index.php?_cmd=catalogue$squery&page=$i'>$i</a></li>";
	}
}

if ($page < $tp) 
{
	$pagesstring .= "<li><a href='index.php?_cmd=catalogue$squery&page=$next'>&raquo;</a></li>";
	$pagesstring .= "<li><a href='index.php?_cmd=catalogue$squery&page=$tp'>Last Page</a></li>";
}

echo ((is_numeric($searchparams['parent_id']) && $searchparams['title'] == "" && $searchparams['coming_soon'] == "" && $searchparams['enabled'] == "") ? '<br /><input type="submit" name="update-order" value="Save page order"> or <input type="button" value="Cancel/Reset" onclick="location.reload(true);"></form>' : '');
echo '<div class="center"><ul class="pagination">' . $pagesstring . '</ul></div>';

require_once "bottom.php";
?>
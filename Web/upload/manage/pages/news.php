<?php

if (!defined('IN_HK') || !IN_HK)
{
	exit;
}

if (!HK_LOGGED_IN || !$GetUsers->HasRight($GetUsers->Name2Id(USER_NAME), 'hk_sitemanagement'))
{
	exit;
}

if (isset($_GET['doDel']) && is_numeric($_GET['doDel']))
{
	mysql_query("DELETE FROM articles WHERE id = '" . intval($_GET['doDel']) . "' LIMIT 1"); 
	
	if (mysql_affected_rows() >= 1)
	{
		fMessage('ok', 'Article deleted.');
	}
	
	header("Location: index.php?_cmd=news&deleteOK");
	exit;
}

if (isset($_GET['doBump']) && is_numeric($_GET['doBump']))
{
	mysql_query("UPDATE articles SET published = '" . time() . "' WHERE id = '" . intval($_GET['doBump']) . "' LIMIT 1"); 
	
	if (mysql_affected_rows() >= 1)
	{
		fMessage('ok', 'Article date bumped.');
	}
	
	header("Location: index.php?_cmd=news&bumpOK");
	exit;
}

require_once "top.php";

?>			
<h1>Manage News</h1>

<br />

<p>
	You can use this overview to manage news articles. Topstories will be <span style="background-color: #CEE3F6; padding: 2px;">highlighted</span>.
</p>

<br />

<p>
	<a href="index.php?_cmd=newspublish">
		<b>
			Write new article
		</b>
	</a>
</p>

<br />

<table border="1" width="100%">
<thead>
<tr>
	<td>ID</td>
	<td>Title</td>
	<td>Topstory snippet</td>
	<td>Category</td>
	<td>Date</td>
	<td>Controls</td>
</tr>
</thead>
<tbody>
<?php

$getNews = mysql_query("SELECT * FROM articles ORDER BY published DESC");
$i = 1;

while ($n = mysql_fetch_assoc($getNews))
{
	$highlight = '#fff';
	
	if ($i <= 3)
	{
		$highlight = '#CEE3F6';
	}
	else if ($i <= 5)
	{
		$highlight = '#EFFBFB';
	}
	
	echo '<tr style="background-color: ' . $highlight . ';">
	<td>' . $n['id'] . '</td>
	<td>' . $n['title'] . '</td>
	<td>' . $n['shortstory'] . '</td>
	<td>' . mysql_result(mysql_query("SELECT caption FROM articles_categories WHERE id = '" . $n['category_id'] . "' LIMIT 1"), 0) . '</td>
	<td>' . date('D, d M Y H:i:s', $n['published']) . '</td>
	<td>
		<input type="button" value="View" onclick="document.location = \'http://' . SITE_DOMAIN . '/news.php?id=' . $n['id'] . '-' . $n['seo_link'] . '\';">&nbsp;
		<input type="button" value="Delete" onclick="document.location = \'index.php?_cmd=news&doDel=' . $n['id'] . '\';">&nbsp;
		<input type="button" value="Edit" onclick="document.location = \'index.php?_cmd=newsedit&u=' . $n['id'] . '\';">
		<input type="button" value="Update/bump date" onclick="document.location = \'index.php?_cmd=news&doBump=' . $n['id'] . '\';">&nbsp;
	</td>
	</tr>';
	
	$i++;
}

?>
</tbody>
</table>


<?php

require_once "bottom.php";

?>
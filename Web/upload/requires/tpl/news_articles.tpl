<?php
/*==========================================================================\
| RainbowCMS - A good CMS option for Snowlight Emulator                     |
| Copyright (C) 2021 - Created by LeoXDR edited by Souza                    |
| https://github.com/LeonSttKennedy/Snowlight-Emu                           |
| ========================================================================= |
| This CMS was developed with the permission of Meth0d                      |
\==========================================================================*/

$query = mysql_query("SELECT * FROM articles ORDER BY published DESC LIMIT 1");
$num = mysql_num_rows($query);
if($num > 0)
{
?>
<div id="newsblock">
<?php
	$news = mysql_fetch_assoc($query);
	TemplateManager::WriteLine('<div class="topnews" style="background-image: url('.$news['image'].');">
		<div class="inner">
		<h4>Latest News</h4>
		<h2>'.$news['title'].'</h2>
		<p class="wrap">'.$news['shortstory'].'</p><p><a href="http://'.SITE_DOMAIN.'/news.php?id=' . $news['id'] . '-' . $news['seo_link'] . '">Read more &raquo;</a></p>
		</div>
		</div>');
?>
	<div class="newslist">
		<ul class="newslist-inner">
		<?php
		$query = mysql_query("SELECT * FROM articles ORDER BY published DESC LIMIT 3");
			while($news = mysql_fetch_assoc($query))
			{
				if($color == 'n')
				$color='alt';
				else
				$color='n';
				TemplateManager::WriteLine('<li class="'.$color.'">
						<a><a href="http://'.SITE_DOMAIN.'/news.php?id=' . $news['id'] . '-' . $news['seo_link'] . '">'.$news['title'].'</a><br />
						<small>'.@date("D, d M Y H:i:s", $news['published']).'</small><br />
					</li>');
			}
TemplateManager::WriteLine('<li class="alt">
			<small><a href="http://'.SITE_DOMAIN.'/news.php'.$news['id'].'">News archive &raquo;</a></small>
		</li>');
?>
		</ul>

	</div>

</div>
<?php
}
?>

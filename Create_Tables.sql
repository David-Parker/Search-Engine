drop table keywords;
drop table Page_rank;

Create table Keywords(
	url varchar(900),
	keyword varchar(255),
	k_count int,
	date datetime,
	guid varchar(900)
);

Create table Page_Rank(
	url varchar(900),
	p_rank int,
	primary key (url)
);


create nonclustered index idx_keywords on keywords (keyword);
create nonclustered index idx_url on page_rank (url);
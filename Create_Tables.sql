drop table keywords;
drop table Page_rank;

-- MOVING DATE TO PAGE RANK
Create table Keywords(
	url varchar(900),
	keyword varchar(255),
	k_count int,
	guid varchar(900)
);

Create table Page_Rank(
	url varchar(900),
	p_rank int,
	date datetime
	primary key (url)
);

create nonclustered index idx_keywords on keywords (keyword);
-- create nonclustered index idx_keywords_url on keywords (url);
create nonclustered index idx_url on page_rank (url);
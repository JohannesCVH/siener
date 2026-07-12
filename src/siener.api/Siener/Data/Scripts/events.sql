create table if not exists events(
	id int generated always as identity primary key,
	camera varchar(100) not null,
	detection_types smallint not null,
	start_time timestamp not null,
	end_time timestamp null,
	notified boolean not null
)

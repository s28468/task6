
CREATE TABLE Animal (
    IdAnimal INT PRIMARY KEY,
    Name NVARCHAR(200),
    Description NVARCHAR(200),
    Category NVARCHAR(200),
    Area NVARCHAR(200)
);

insert into Animal values (1, 'q1', 'w', 'e','r')
insert into Animal values (2, 'q2', 'w', 'e','r')
insert into Animal values (3, 'q3', 'w', 'e','r')
insert into Animal values (4, 'q4', 'w', 'e','r')

select * from Animal
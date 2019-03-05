create extension if not exists pgcrypto;

create table profiles (
  entity uuid primary key,
  screen_name text not null unique,
  email text not null unique
);
# MySQL volume credentials

`MYSQL_PASSWORD` is used by the MySQL image only when the database volume is initialized. On later starts, changing the `.env` value does not change the existing MySQL user's password.

If an existing volume rejects the `.env` password, preserve the volume and run Compose with the already-established password supplied through the process environment or a secret manager. Do not put that value in source control and do not delete `tourguidenarration_db_data` to bypass the mismatch.

import sqlite3 as sql
from sqlite3 import Error
OLD_DB_PATH = 'weather.db'
NEW_DB_PATH = 'weather_data.db'

def connect(db_file):
    '''
    create a database connection to the SQLite database 
    specified by db_file
    :param db_file: database file
    :return: Connection object or None
    '''
    conn = None
    try:
        conn = sql.connect(db_file)
    except Error as e:
        print(e)

    return conn


def database_export(old_connection, new_connection):
    '''
    Query tasks by priority
    :param connection: Connection to the database
    :return:
    '''
    sql_devices = '''
        SELECT *
        FROM devices
    '''
    sql_pydata = '''
        SELECT * 
        FROM py_data
    '''
    sql_lhtdata = '''
        SELECT *
        FROM lht_data
    '''
    
    
    
    cur = old_connection.cursor()
    cur.execute(sql_devices)
    devices_table = cur.fetchall()
    for row in devices_table:
        put_devices(new_connection, row)
        
    cur = old_connection.cursor()
    cur.execute(sql_pydata)
    pydata_table = cur.fetchall()
    for row in pydata_table:
        put_py_data(new_connection, row)
    
    cur = old_connection.cursor()
    cur.execute(sql_lhtdata)
    lhtdata_table = cur.fetchall()
    for row in lhtdata_table:
        put_lht_data(new_connection, row)


def put_py_data(connection, data):
    query = '''
        INSERT INTO py_data
        VALUES(?, ?, NULL, ?, ?, ?, ?)
    '''
    cur = connection.cursor()
    cur.execute(query, data)
    connection.commit()


def put_lht_data(connection, data):
    query = '''
        INSERT INTO lht_data
        VALUES(?, ?, NULL, ?, ?, ?, ?, ?)
    '''
    cur = connection.cursor()
    cur.execute(query, data)
    connection.commit()


def put_devices(connection, data):
    query = '''
        INSERT INTO devices(id, type, location)
        VALUES(?, ?, ?)
    '''
    cur = connection.cursor()
    cur.execute(query, data)
    connection.commit()  

def main():
    old_db = connect(OLD_DB_PATH)
    new_db = connect(NEW_DB_PATH)
    with old_db and new_db:
        database_export(old_db, new_db)


if __name__ == '__main__':
    main()
import sqlite3
from sqlite3 import Error
import base64

class Sql_writer:
    '''!
    class that is used for writing to the database

    Class has put_py_data and put_lht_data public functions
    that could be used for writing the py or the lht data to the database
    '''
    def __init__(self, db_file):
        '''!
        constructor that connects to the database at db_file path

        @param self
        @param db_file path to the database file
        '''
        self.__connect(db_file)

    def __connect(self, db_file):
        '''!
        create a database connection to the SQLite database, which is at the path, 
        specified by db_file
        
        @param db_file path to the database file
        '''
        self.__connection = None
        try:
            self.__connection = sqlite3.connect(db_file)
        except Error as e:
            print(e)
            exit()


    def __put_device(self, payload_dict):
        '''!
        private funciton for putting the device to the device table of the database

        @param self
        @param payload_dict the information, received from MQTT sesrver
        '''
        device_query = '''
        INSERT INTO devices
        VALUES(?, ?, ?)
        '''
        try:
            device_id = payload_dict['end_device_ids']['device_id']
        except (Error, Exception) as E:
            print(E)
            print(payload_dict)
            raise ValueError('no device_id inside the payload_dict')
        try:
            device_type = device_id.split('-')[0]
            device_location = device_id.split('-')[1]
        except Exception as E:
            print(f'Bad device!{device_id}')
            print(payload_dict)
            raise ValueError('Bad device name')

        cursor = self.__connection.cursor()
        try:
            cursor.execute(device_query, (device_id, device_type, device_location))
        except Error as e:
            pass

        self.__connection.commit()
        return device_id
    

    def __put_metadata(self, payload_dict):
        '''!
        private function for putting metadata into the database and returning the metadata_id

        @param self
        @param payload_dict the information, received from MQTT sesrver
        '''
        metadata_get_id_query = '''
        SELECT id 
        FROM metadata
        ORDER BY id DESC
        LIMIT 1
        '''
        cursor = self.__connection.cursor()
        cursor.execute(metadata_get_id_query)
        try:
            metadata_id = cursor.fetchall()[0][0]
        except Exception as e:
            metadata_id = 0
        metadata_id += 1

        metadata_query = '''
            INSERT INTO metadata
            VALUES(?, ?, ?)
        '''
        try:
            for metadata in payload_dict['uplink_message']['rx_metadata']:
                data = (metadata_id, metadata['gateway_ids']['gateway_id'], metadata['rssi'])
                cursor = self.__connection.cursor()
                cursor.execute(metadata_query, data)
                self.__connection.commit()
        except (Exception, Error) as e:
            print(e)
            print(payload_dict)
            raise ValueError('something went wrong with metadata. Check the payload_dict')
        
        return metadata_id



    def __put_py_data(self, payload_dict):
        '''!
        private funciton for putting the py data to the py_data table of the database

        @param self
        @param payload_dict the information, received from MQTT sesrver
        '''
        data_query = '''
        INSERT INTO py_data
        VALUES(?, datetime('now'), ?, ?, ?, ?, ?)
        '''
        
        try:
            device_id = self.__put_device(payload_dict)
        except (Error, Exception) as e:
            print('py_data')
            print(e)
            return

        try: 
            metadata_id = self.__put_metadata(payload_dict)
        except (Error, Exception) as e:
            print('py_data')
            print(e)
            return

        try:
            encoded_data = payload_dict['uplink_message']['frm_payload']
        except (Error, Exception) as E:
            print(E)
            return
        
        str = base64.b64decode(encoded_data[0:4])
        pressure = 950 + str[0] / 2
        light = str[1]
        temperature = str[2] - 20 
        str = base64.b64decode(encoded_data[4:8])
        temperature = (temperature * 10 + str[0]) / 10 

        cursor = self.__connection.cursor()
        try:
            cursor.execute(data_query, (device_id, metadata_id, temperature, light, pressure, encoded_data))
        except (Exception, Error) as E:
            print(E)
            print(payload_dict)
        
        self.__connection.commit()


    def __put_lht_data(self, payload_dict):
        '''!
        private funciton for putting the lht data to the lht_data table of the database

        @param self
        @param payload_dict the information, received from MQTT sesrver
        '''
        data_query = '''
        INSERT INTO lht_data
        VALUES(?, datetime('now'), ?, ?, ?, ?, ?, ?)
        '''
        try:
            device_id = self.__put_device(payload_dict)
        except (Error, Exception) as e:
            print('lht_data')
            print(e)
            return
        
        try: 
            metadata_id = self.__put_metadata(payload_dict)
        except (Error, Exception) as e:
            print('lht_data')
            print(e)
            return

        try:
            encoded_data = payload_dict['uplink_message']['frm_payload']
        except (Error, Exception) as E:
            print(E)
            return
        
        str1 = base64.b64decode(encoded_data[0:4])
        str2 = base64.b64decode(encoded_data[4:8])
        str3 = base64.b64decode(encoded_data[8:12])

        battery_voltage = ((str1[0] << 8 | str1[1]) & (0x3FFF))/1000
        temperature = ((str1[2] << 24 >> 16 | str2[0])/100)
        humidity = (str2[1] << 8 | str2[2]) / 10
        illumination = str3[1] << 8 | str3[2]

        cursor = self.__connection.cursor()
        try: 
            cursor.execute(data_query, (device_id, metadata_id, temperature, illumination, humidity, battery_voltage, encoded_data))
        except (Exception, Error) as E:
            print(E)
            print(payload_dict)

        self.__connection.commit()
    

    def put_data(self, payload_dict):
        '''!
        public funciton for putting the sensors data to the py or lht data table of the database

        @param self
        @param payload_dict the information, received from MQTT sesrver
        '''
        if payload_dict['end_device_ids']['device_id'].split('-')[0] == 'py':
            self.__put_py_data(payload_dict)
        elif payload_dict['end_device_ids']['device_id'].split('-')[0] == 'lht':
            self.__put_lht_data(payload_dict)
        else: 
            try:
                print(payload_dict['end_device_ids']['device_id'])
            except Exception as e:
                print(e)
                print(payload_dict)
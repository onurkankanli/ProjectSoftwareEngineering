import sqlite3
from sqlite3 import Error

import datetime

import json

class Sql_getter:
    __get_all_query = ['''
        SELECT py_data.device_id, py_data.date, py_data.temperature, py_data.light, py_data.pressure, round(AVG(metadata.signal_strength), 2)
        FROM py_data
        INNER JOIN metadata
        ON py_data.metadata_id = metadata.id
        WHERE date >= ? AND date < ? AND metadata_id is not Null
        GROUP BY date
        ORDER BY device_id
        
    ''','''
        SELECT lht_data.device_id, lht_data.date, lht_data.temperature, lht_data.illumination, lht_data.humidity, lht_data.battery_voltage, round(AVG(metadata.signal_strength), 2)
        FROM lht_data
        INNER JOIN metadata
        ON lht_data.metadata_id = metadata.id
        WHERE date >= ? AND date < ? AND metadata_id is not Null
        GROUP BY date
        ORDER BY device_id
    ''']
    __queries = ['''
                SELECT device_id, AVG(temperature), AVG(light), AVG(pressure), AVG(metadata.signal_strength)
                FROM py_data
                INNER JOIN metadata
                ON metadata.id = py_data.metadata_id
                WHERE py_data.date >= ? and py_data.date < ?
                GROUP BY device_id
            ''', '''
                    SELECT device_id, AVG(temperature), AVG(illumination), AVG(humidity), AVG(battery_voltage), AVG(metadata.signal_strength)
                    FROM lht_data
                    INNER JOIN metadata
                    ON metadata.id = lht_data.metadata_id
                    WHERE lht_data.date >= ? and lht_data.date < ?
                    GROUP BY device_id
                ''']
    __queries_no_metadata = ['''
                SELECT device_id, AVG(temperature), AVG(light), AVG(pressure)
                FROM py_data
                WHERE py_data.date >= ? and py_data.date < ?
                GROUP BY device_id
            ''', '''
                    SELECT device_id, AVG(temperature), AVG(illumination), AVG(humidity), AVG(battery_voltage)
                    FROM lht_data
                    WHERE lht_data.date >= ? and lht_data.date < ?
                    GROUP BY device_id
                ''']
    __metadata_check = ['''
                SELECT AVG(metadata_id)
                FROM py_data
                WHERE date >= ? and date < ?
            ''', '''
                    SELECT AVG(metadata_id)
                    FROM lht_data
                    WHERE date >= ? and date < ?
                ''']
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

    
    def get_data(self, start_date, end_date, data_type, data_points):
        cursor =  self.__connection.cursor()

        template = '%Y-%m-%d %H:%M:%S'
        start_date = datetime.datetime.strptime(start_date, template) - datetime.timedelta(hours=1)
        end_date = datetime.datetime.strptime(end_date, template) - datetime.timedelta(hours=1)



        data_points = int(data_points)
        data_point_duration = self.__get_time_delta(start_date, end_date, data_points)
        returning_data = {'Temperature': {}, 'Light': {}, 'Pressure': {}, 'Humidity': {}, 'BatteryVoltage': {}, 'SignalStrength': {}}

        cursor.execute(self.__get_all_query[0], (start_date, end_date))
        py_data = cursor.fetchall()

        cursor.execute(self.__get_all_query[1], (start_date, end_date))
        lht_data = cursor.fetchall()

        data_dots = []
        data_averaged = []
        for data_point in range(data_points):
            data_dots.append(str(start_date + datetime.timedelta(seconds=data_point_duration * data_point) + datetime.timedelta(hours=1)))
            data_averaged.append({'timestamp': data_dots[-1], 'py_sensors': {}, 'lht_sensors': {}})
        data_dots.append(end_date)

        # print(data_averaged)

        for data in py_data:
            # 0-2  2-4   4-6   6-8    8-10   10-12
            # 2 
            # 2022-01-10 00:00:00; 2022-01-10 12:00:00     2022-01-10 11:12:45
            
            time_index = ((datetime.datetime.strptime(data[1], template) - start_date).days * 24 * 3600 + (datetime.datetime.strptime(data[1], template) - start_date).seconds)//int(data_point_duration)
            time_index = int(time_index)
            try:
                data_averaged[time_index]['py_sensors'][data[0]].append({'Temperature': data[2], 'Light': data[3], 'Pressure': data[4], 'SignalStrength': data[5]})
            except Exception:
                data_averaged[time_index]['py_sensors'][data[0]] = [{'Temperature': data[2], 'Light': data[3], 'Pressure': data[4], 'SignalStrength': data[5]}]

        for data in lht_data:
            time_index = ((datetime.datetime.strptime(data[1], template) - start_date).days * 24 * 3600 + (datetime.datetime.strptime(data[1], template) - start_date).seconds)//int(data_point_duration)
            time_index = int(time_index)
            try:
                data_averaged[time_index]['lht_sensors'][data[0]].append({'Temperature': data[2], 'Light': data[3], 'Humidity': data[4], 'BatteryVoltage': data[5], 'SignalStrength': data[6]})
            except Exception:
                data_averaged[time_index]['lht_sensors'][data[0]] = [{'Temperature': data[2], 'Light': data[3], 'Humidity': data[4], 'BatteryVoltage': data[5], 'SignalStrength': data[6]}]
            
        # print(data_averaged, end='\n\n\n')


        for data_in_range in data_averaged:
            for sensor in data_in_range['py_sensors']:
                count = 0
                try:
                    returning_data['Temperature'][sensor].append({'timestamp': data_in_range['timestamp'], 'value': 0.})
                    returning_data['Light'][sensor].append({'timestamp': data_in_range['timestamp'], 'value': 0.})
                    returning_data['Pressure'][sensor].append({'timestamp': data_in_range['timestamp'], 'value': 0.})
                    returning_data['SignalStrength'][sensor].append({'timestamp': data_in_range['timestamp'], 'value': 0.})
                except Exception:
                    returning_data['Temperature'][sensor] = [{'timestamp': data_in_range['timestamp'], 'value': 0.}]
                    returning_data['Light'][sensor] = [{'timestamp': data_in_range['timestamp'], 'value': 0.}]
                    returning_data['Pressure'][sensor] = [{'timestamp': data_in_range['timestamp'], 'value': 0.}]
                    returning_data['SignalStrength'][sensor] = [{'timestamp': data_in_range['timestamp'], 'value': 0.}]
                for sensor_data in  data_in_range['py_sensors'][sensor]:
                    # print(sensor_data, end='\n\n')
                    returning_data['Temperature'][sensor][-1]['value'] += sensor_data['Temperature']
                    returning_data['Light'][sensor][-1]['value'] += sensor_data['Light']
                    returning_data['Pressure'][sensor][-1]['value'] += sensor_data['Pressure']
                    returning_data['SignalStrength'][sensor][-1]['value'] += sensor_data['SignalStrength']
                    count += 1        
                returning_data['Temperature'][sensor][-1]['value'] = round(returning_data['Temperature'][sensor][-1]['value'] / count, 2)
                returning_data['Light'][sensor][-1]['value'] = round(returning_data['Light'][sensor][-1]['value'] / count, 2)
                returning_data['Pressure'][sensor][-1]['value'] = round(returning_data['Pressure'][sensor][-1]['value'] / count, 2)
                returning_data['SignalStrength'][sensor][-1]['value'] = round(returning_data['SignalStrength'][sensor][-1]['value'] / count, 2)
            

            for sensor in data_in_range['lht_sensors']:
                count = 0
                try:
                    returning_data['Temperature'][sensor].append({'timestamp': data_in_range['timestamp'], 'value': 0.})
                    returning_data['Light'][sensor].append({'timestamp': data_in_range['timestamp'], 'value': 0.})
                    returning_data['Humidity'][sensor].append({'timestamp': data_in_range['timestamp'], 'value': 0.})
                    returning_data['BatteryVoltage'][sensor].append({'timestamp': data_in_range['timestamp'], 'value': 0.})
                    returning_data['SignalStrength'][sensor].append({'timestamp': data_in_range['timestamp'], 'value': 0.})
                except Exception:
                    returning_data['Temperature'][sensor] = [{'timestamp': data_in_range['timestamp'], 'value': 0.}]
                    returning_data['Light'][sensor] = [{'timestamp': data_in_range['timestamp'], 'value': 0.}]
                    returning_data['Humidity'][sensor] = [{'timestamp': data_in_range['timestamp'], 'value': 0.}]
                    returning_data['BatteryVoltage'][sensor] = [{'timestamp': data_in_range['timestamp'], 'value': 0.}]
                    returning_data['SignalStrength'][sensor] = [{'timestamp': data_in_range['timestamp'], 'value': 0.}]
                for sensor_data in  data_in_range['lht_sensors'][sensor]:
                    # print(sensor_data, end='\n\n')
                    returning_data['Temperature'][sensor][-1]['value'] += sensor_data['Temperature']
                    returning_data['Light'][sensor][-1]['value'] += sensor_data['Light']
                    returning_data['Humidity'][sensor][-1]['value'] += sensor_data['Humidity']
                    returning_data['BatteryVoltage'][sensor][-1]['value'] += sensor_data['BatteryVoltage']
                    returning_data['SignalStrength'][sensor][-1]['value'] += sensor_data['SignalStrength']
                    count += 1        
                returning_data['Temperature'][sensor][-1]['value'] = round(returning_data['Temperature'][sensor][-1]['value'] / count, 2)
                returning_data['Light'][sensor][-1]['value'] = round(returning_data['Light'][sensor][-1]['value'] / count, 2)
                returning_data['Humidity'][sensor][-1]['value'] = round(returning_data['Humidity'][sensor][-1]['value'] / count, 2)
                returning_data['BatteryVoltage'][sensor][-1]['value'] = round(returning_data['BatteryVoltage'][sensor][-1]['value'] / count, 2)
                returning_data['SignalStrength'][sensor][-1]['value'] = round(returning_data['SignalStrength'][sensor][-1]['value'] / count, 2)
        if data_type == 'All':
            return json.dumps(returning_data)
        else:
            return json.dumps(returning_data[data_type])

    def __get_time_delta(self, start_date, end_date, data_points):
        return ((end_date - start_date).days * 24 * 60 * 60 + (end_date - start_date).seconds)// int(data_points)

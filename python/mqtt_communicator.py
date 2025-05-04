import paho.mqtt.client as mqtt
import json

class MQTT_communicator:
    '''!
    Class for listening and getting the data from MQTT

    Class connects to the given MQTT server with the given unsername and password, 
    subscribes to the server and every message calls function for writing data to the database
    '''
    # initialising constatnts used for mqtt
    __MQTT_SERVER = "eu1.cloud.thethings.network"
    __MQTT_PATH = "v3/project-software-engineering@ttn/devices/#"
    __MQTT_USERNAME = "project-software-engineering@ttn"
    __MQTT_PASSWORD = "NNSXS.DTT4HTNBXEQDZ4QYU6SG73Q2OXCERCZ6574RVXI.CQE6IG6FYNJOO2MOFMXZVWZE4GXTCC2YXNQNFDLQL4APZMWU6ZGA"
    def __init__(self, sql_writer):
        '''!
        constructor of the class that initializes the client

        @param self
        @param sql_writer instance of class Sql_Writer

        @see sql_writer
        '''
        self.__sql_writer = sql_writer
        self.__client = mqtt.Client()
        # putting our functions on_connect and on_message to the MQTT client
        self.__client.on_connect = self.__on_connect
        self.__client.on_message = self.__on_message
        # connecting to the server
        self.__client.username_pw_set(self.__MQTT_USERNAME, self.__MQTT_PASSWORD)
        self.__client.connect(self.__MQTT_SERVER, 1883, 60)


    def __on_connect(self, client, userdata, flags, rc):
        '''!
        subscrbes to the server, when the client receives a connect response from it. 

        
        @param self
        @param client 
        @param userdata
        @param flags
        @param rc the result code of the connection
        '''
        print("Connected with result code "+str(rc))
        # on_connect() means that if we lose the connection and reconnect then subscriptions will be renewed.
        client.subscribe(self.__MQTT_PATH)


    def __on_message(self, client, userdata, msg):
        '''!
        puts the data into the database, when a PUBLISH message is received from the server.
        '''
        # parsing the data from json to python dictionary
        payload_dict = json.loads(msg.payload)
        self.__sql_writer.put_data(payload_dict)

    def loop(self):
        '''!
        tells the client to run forever

        @param self
        '''
        self.__client.loop_forever()
from sql_writer import Sql_writer
from mqtt_communicator import MQTT_communicator


class Controller:
    '''!
    Class that puts everything together.

    Class has run() function, which should be called,
    when it is needed to start listening to the server
    '''
    
    # constant for path to the database
    __DB_PATH = '../database/weather_data.db'
    def __init__(self):
        '''!
        constructor of the class that initialises member function of MQTT_communicator type

        @param self
        
        @see mqtt_communicator
        '''
        self.__mqtt_communicator = MQTT_communicator(Sql_writer(self.__DB_PATH))
    

    def run(self):
        '''!
        starts listening to the server

        @param self
        '''
        self.__mqtt_communicator.loop()

    
    
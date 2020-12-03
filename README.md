# NC2
# This console program is intended to be used for filtering NMEA messages from GNSS communication.
# The program gets a text file with all the messages and writes the selected NMEA messages to a separate CSV file.
#
# The main features are:
# a) Filter out location fix (GGA) messages.
# b) Automatically add colons to message timestamp.
# c) Automatically convert latitude and longitude values from DDMM.MMMM to DD.DDDD
# b) Optionally include a signal-to-noise ratio gathered from GSV messages as an attribute to each exported GGA message.

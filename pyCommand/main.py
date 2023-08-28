import matplotlib.pyplot as plt,keyboard,time,requests


def get_data_from_api(api_route):
    try:
        response = requests.get(api_route)
        response.raise_for_status() 
        # Raise an exception for 4xx and 5xx status codes!
        data = response.json()
        return data
    except requests.exceptions.RequestException as e:
        print("Error fetching data:", e)
        return None


api_route = input("Enter the API route: ")

count = 1
x_values = []
y_values = []

while True:
    launch = get_data_from_api(api_route+'/ngforce/ignition')
    if (keyboard.is_pressed('L') or keyboard.is_pressed('l')):
        get_data_from_api(api_route+'/ngforce/ignition/1')
    if launch:
        data = get_data_from_api(api_route+'/ngforce/engine/last')
        if data is not None:
            x_values.append(count)
            y_values.append(data)
            count += 1
            print("Thrust: ", data)
        else:
            print("No entry has been made!")
    if (keyboard.is_pressed('Q') or keyboard.is_pressed('q')):
        plt.plot(x_values,y_values)
        plt.show()
        print("// Terminated!")
        break
    if (keyboard.is_pressed('P') or keyboard.is_pressed('p')):
        last100 = get_data_from_api(api_route+'/ngforce/engine')
        plt.plot(range(1,len(last100)+1),last100)
        plt.show()

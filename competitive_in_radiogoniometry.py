# -*- coding: utf-8 -*-
"""
Created on Thu Apr 18 19:48:30 2024

@author: Admin
"""

import serial
import time
import random

# Definiranje serijskog porta
ser = serial.Serial('COM7', 9600)  # Promijenite 'COM7' u odgovarajući serijski port

# Čekanje nekoliko sekundi kako bi se serijski port stabilizirao
time.sleep(2)

# Lista ciljeva koje možemo poslati Arduinu
ciljevi = ['A', 'B', 'C']

# Čekanje na početak takmičenja
input("Pritisnite Enter za početak takmičenja...")

# Pokretanje vremena od početka takmičenja
pocetno_vrijeme = time.time()

# Vremensko limit za takmičenje (5 minuta = 300 sekundi)
vremenski_limit = 300  # sekunde

# Pomoćna varijabla za čuvanje prvog izabranog predajnika
prvi_predajnik = None

# Glavna petlja takmičenja
pronadjeni_ciljevi = []


while len(pronadjeni_ciljevi) < 3:
    
    # Provjera vremena takmičenja
    trenutno_vrijeme = time.time()
    if trenutno_vrijeme - pocetno_vrijeme > vremenski_limit:
        print("Diskvalifikacija: Vrijeme je isteklo, niste pronašli sve predajnike u roku.")
        
        # Slanje naredbe Arduino-u da isključi predajnik
        ser.write('X'.encode())
        
        break
    
    # Odabir nasumičnog cilja iz preostalih ciljeva
    cilj = random.choice(ciljevi)
    
    # Slanje odabranog cilja na Arduina
    ser.write(cilj.encode())
    time.sleep(0.000001)
    
    # Postavljanje timeout-a na serijskom portu
    ser.timeout = vremenski_limit - (time.time() - pocetno_vrijeme)
    #time.sleep(1)
    
    response = ser.readline().decode().strip()
    
    # Provjera je li pronadjen novi cilj
    if response.startswith("Predajnik") and response.endswith("pronadjen") and response.split()[1] == cilj:
        # Izračunaj vrijeme proteklo od početka takmičenja do pronalaska trenutnog predajnika
        vrijeme_pronalaska = time.time() - pocetno_vrijeme
        
        # Ispis vremena pronalaska za trenutni predajnik
        print(f"Pronađen predajnik {cilj} za {vrijeme_pronalaska:.2f} sekundi.")
        
        # Ako je ovo prvi pronadjeni predajnik, spremi ga u pomoćnu varijablu
        if len(pronadjeni_ciljevi) == 0:
            prvi_predajnik = cilj
        
        # Dodaj pronadjeni predajnik u listu pronadjenih ciljeva
        pronadjeni_ciljevi.append(cilj)
        
        # Ukloni pronadjeni cilj iz liste ciljeva
        ciljevi.remove(cilj)
    
    # Pauza između svake iteracije
    time.sleep(2)
    
    # Ako su pronadjeni svi ciljevi osim zadnjeg
    if len(pronadjeni_ciljevi) == 2 and len(ciljevi) == 1:
        # Dodaj prvog izabranog predajnika nazad u listu ciljeva prije biranja posljednjeg preostalog predajnika
        ciljevi.append(prvi_predajnik)
    
# Ispis ukupnog vremena trajanja takmičenja
ukupno_vrijeme_takmicenja = time.time() - pocetno_vrijeme - 2
print(f"Ukupno vrijeme takmičenja: {ukupno_vrijeme_takmicenja:.2f} sekundi")

# Završetak takmičenja
print("Takmičenje je završeno!")

# Zatvaranje serijskog porta
ser.close()